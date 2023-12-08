#include "KuavteFdmModel.h"

#include <chrono>

KuavteFdmModel::KuavteFdmModel()
    :FDMLogFile("FDMLog.txt")
{
    Log("Construction process begins");

    FDMLogFile.clear();

    if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0) {
        Log("Failed to initialize winsock. Error Code = " + std::to_string(WSAGetLastError()));

        CloseLogFile();

        exit(1);
    }

    udpSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);

    if (udpSocket == INVALID_SOCKET) {
        Log("Failed to create socket. Error Code = " + std::to_string(WSAGetLastError()));

        CloseLogFile();

        WSACleanup();

        exit(1);
    }

    serverAddr.sin_family = AF_INET;
    serverAddr.sin_port = htons(PORT);
    serverAddr.sin_addr.s_addr = inet_addr("127.0.0.1");

    Log("Construction successfully completed");

    Init();
}

KuavteFdmModel::~KuavteFdmModel()
{
    Deinit();
}

void KuavteFdmModel::Init()
{
    Log("Initialization started");

    Log("Initialization successfully completed");
}

void KuavteFdmModel::Deinit()
{
    Log("Deinitialization process begins");

    closesocket(udpSocket);

    WSACleanup();

    CloseLogFile();

    Log("Deinitialization successfully completed");
}

void KuavteFdmModel::InitializeFDMEnvironment()
{
    Log("Starting FDM environment initialization");

    this->modelPath = SGPath(JSBSIM_ROOT_DIR);

    if(fdm != nullptr){
        delete fdm;
    }

    Log("Creating fdm executor");
    fdm = new JSBSim::FGFDMExec();

    Log("Setting FDM aircraft directory");
    fdm->SetAircraftPath(SGPath(JSBSIM_AIRCRAFT_DIR));
    Log("Setting FDM engine directory");
    fdm->SetEnginePath(SGPath(JSBSIM_ENGINE_DIR));
    Log("Setting FDM system directory");
    fdm->SetSystemsPath(SGPath(JSBSIM_SYSTEM_DIR));
    Log("Setting FDM root directory");
    fdm->SetRootDir(SGPath(JSBSIM_ROOT_DIR));

    Log("FDM loading F450 model");
    fdm->LoadModel(AIRCRAFT_NAME);

    // Initial conditions
    fdm->SetPropertyValue("ic/vc-kts", 0);
    fdm->SetPropertyValue("ic/ve-kts", 0);
    fdm->SetPropertyValue("ic/vg-kts", 0);
    fdm->SetPropertyValue("ic/vt-kts", 0);
    fdm->SetPropertyValue("ic/gamma-deg", 0);
    fdm->SetPropertyValue("ic/alpha-deg", 0);
    fdm->SetPropertyValue("ic/beta-deg", 0);
    fdm->SetPropertyValue("ic/theta-deg", 0);
    fdm->SetPropertyValue("ic/phi-deg", 0);
    fdm->SetPropertyValue("ic/psi-true-deg", 0);
    fdm->SetPropertyValue("ic/terrain-elevation-ft", 0);

    Log("FDM running initial conditions");
    fdm->RunIC();

    Log("FDM preparing throttle");
    fdm->SetPropertyValue("fcs/throttle-cmd-norm", 0.0);
    fdm->SetPropertyValue("fcs/throttle-cmd-norm[1]", 0.0);
    fdm->SetPropertyValue("fcs/throttle-cmd-norm[2]", 0.0);
    fdm->SetPropertyValue("fcs/throttle-cmd-norm[3]", 0.0);

    fdm->Run();

    Log("FDM ground trim starting");
    fdm->DoTrim(2);

    while(fdm->GetPropertyValue("simulation/sim-time-sec") < 2.0){
        fdm->Run();
    }

    Log("FDM scas engaging");
    fdm->SetPropertyValue("fcs/ScasEngage", 1);

    Log("FDM environment initialization completed");
}

void KuavteFdmModel::Log(std::string data)
{
    FDMLogFile << data << std::endl;
}

void KuavteFdmModel::SetTRPY(float throttle, float roll, float pitch, float yaw)
{
    if(fdm != nullptr){
        fdm->SetPropertyValue("fcs/cmdHeave_nd", throttle);
        fdm->SetPropertyValue("fcs/elevator-cmd-norm", pitch);
        fdm->SetPropertyValue("fcs/rudder-cmd-norm", yaw);
        fdm->SetPropertyValue("fcs/aileron-cmd-norm", roll);
    }
}

void KuavteFdmModel::StartFDMEnvironment(float frequency, bool realtime)
{
    Log("Starting FDM Environment");

    std::lock_guard<std::mutex> lock(FDMThreadMutex);

    if(frequency > 25){
        this->freq = frequency;
    }
    else{
        this->freq = 25;
    }

    this->rt = realtime;

    if(isRunning){
        StopFDMEnvironment();
    }

    InitializeFDMEnvironment();

    isRunning = true;
    FDMThread = std::thread(&KuavteFdmModel::FDMThreadFunction, this);

    Log("Completed FDM Environment Starting");
}

void KuavteFdmModel::StopFDMEnvironment()
{
    std::lock_guard<std::mutex> lock(FDMThreadMutex);

    if(isRunning){
        isRunning = false;
        FDMThread.join();
    }
}

void KuavteFdmModel::CloseLogFile()
{
    FDMLogFile.close();
}

void KuavteFdmModel::SendPosition()
{
    // Send position and attitude data of the drone
    static PositionData sendData;

    sendData.x = fdm->GetPropertyValue("position/distance-from-start-lon-mt");
    sendData.y = fdm->GetPropertyValue("position/distance-from-start-lat-mt");

    if(fdm->GetPropertyValue("position/long-gc-rad") < 0 ){
        sendData.x *= -1;
    }

    if(fdm->GetPropertyValue("position/lat-gc-rad") < 0 ){
        sendData.y *= -1;
    }

    sendData.z = fdm->GetPropertyValue("position/h-sl-meters");

    sendData.phi = fdm->GetPropertyValue("attitude/phi-rad") * 57.2957795f;
    sendData.theta = fdm->GetPropertyValue("attitude/theta-rad") * 57.2957795f;
    sendData.psi = fdm->GetPropertyValue("attitude/psi-rad") * 57.2957795f;

    int bytes = sendto(udpSocket, reinterpret_cast<const char*>(&sendData), sizeof(PositionData), 0, reinterpret_cast<sockaddr*>(&serverAddr), sizeof(serverAddr));
}

void KuavteFdmModel::FDMThreadFunction()
{
    auto next = chrono::steady_clock::now();

    auto step = std::chrono::milliseconds(static_cast<long long>(1000.0 / freq));

    auto prev = next - step;

    while(isRunning){
        if(fdm != nullptr){
            // if realtime
            if(rt){

            }
            else{
                fdm->Run();
            }

            SendPosition();
        }
        else{
            // Stop thread is FDM is not initialized
            isRunning = false;
        }

        // do stuff
        auto now = chrono::steady_clock::now();
        prev = now;

        // delay until time to iterate again
        next += step;
        std::this_thread::sleep_until(next);
    }
}

extern "C" DLLExport KuavteFdmModel* FDMCreate(){ 
    return new KuavteFdmModel();
}

extern "C" DLLExport void FDMDelete(KuavteFdmModel* model){
    if(model != NULL){
        delete model;
        model = NULL;
    }
}

extern "C" DLLExport void FDMStartEnvironment(KuavteFdmModel* model, float frequency, bool realtime){
    if(model != NULL){
        model->StartFDMEnvironment(frequency, realtime);
    }
}

extern "C" DLLExport void FDMStopEnvironment(KuavteFdmModel* model){
    if(model != NULL){
        model->StopFDMEnvironment();
    }
}

extern "C" DLLExport void FDMSetTRPY(KuavteFdmModel* model, float throttle, float roll, float pitch, float yaw){
    if(model != NULL){
        model->SetTRPY(throttle, roll, pitch, yaw);
    }
}