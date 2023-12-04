#include "KuavteFdmModel.h"

KuavteFdmModel::KuavteFdmModel()
    :FDMLogFile("FDMLog.txt")
{
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
    serverAddr.sin_port = htons(12345);
    serverAddr.sin_addr.s_addr = inet_addr("127.0.0.1");
}

KuavteFdmModel::~KuavteFdmModel()
{
    
}

void KuavteFdmModel::Init()
{
}

void KuavteFdmModel::Deinit()
{
    closesocket(udpSocket);

    WSACleanup();

    CloseLogFile();
}

void KuavteFdmModel::Log(std::string data)
{
    FDMLogFile << data << std::endl;
}

void KuavteFdmModel::CloseLogFile()
{
    FDMLogFile.close();
}

void KuavteFdmModel::SendData(unsigned int value)
{
    GymEnvironmentInput sendData;
    sendData.initialize = value;
    sendData.throttle = 111.111f;
    sendData.pitch = 222.222f;
    sendData.yaw = 333.333f;
    sendData.roll = 444.444f;

    sendto(udpSocket, reinterpret_cast<const char*>(&sendData), sizeof(GymEnvironmentInput), 0, reinterpret_cast<sockaddr*>(&serverAddr), sizeof(serverAddr));
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

extern "C" DLLExport void FDMInit(KuavteFdmModel* model){
    if(model != NULL){
        model->Init();
    }
}

extern "C" DLLExport void FDMDeinit(KuavteFdmModel* model){
    if(model != NULL){
        model->Deinit();
    }
}

extern "C" DLLExport void FDMSendData(KuavteFdmModel* model, unsigned int data){
    if(model != NULL){
        model->SendData(data);
    }
}
