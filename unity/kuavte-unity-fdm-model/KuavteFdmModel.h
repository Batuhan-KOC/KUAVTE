#pragma once

// STANDARD LIBRARIES
#include <winsock2.h>
#include <iostream>
#include <fstream>
#include <thread>
#include <mutex>
#include <string>

// UDP COMMUNICATION
#pragma comment(lib,"ws2_32.lib") // Winsock Library
#pragma warning(disable:4996) 
#define BUFLEN 512
#define PORT 12345

// FDM
#include "FGFDMExec.h"
#define AIRCRAFT_NAME "F450"
#define JSBSIM_ROOT_DIR "Assets/models"
#define JSBSIM_SYSTEM_DIR "Assets/models/systems"
#define JSBSIM_AIRCRAFT_DIR "Assets/models/aircraft"
#define JSBSIM_ENGINE_DIR "Assets/models/engine"

#define DLLExport __declspec(dllexport)

struct PositionData{
    float x, y, z;
    float phi, theta, psi;
};

class DLLExport KuavteFdmModel{
public:
    KuavteFdmModel();
    ~KuavteFdmModel();

    void Init();
    void Deinit();

    // Clear previous FDM environment values and initialize new one
    void InitializeFDMEnvironment();

    // Receive Throttle Roll Pitch Yaw values
    void SetTRPY(float throttle, float roll, float pitch, float yaw);

    // Start FDM environment
    void StartFDMEnvironment(float frequency = 25, bool realtime = false);

    // Stop FDM Environment
    void StopFDMEnvironment();

    // Send udp position data to the unity environment (x, y, height, psi, phi, theta)
    void SendPosition();

private:
    // FLIGHT DYNAMICS MODEL
    JSBSim::FGFDMExec* fdm{nullptr};

    SGPath modelPath;

    bool rt;

    float freq;

    bool isRunning{false};

    std::thread FDMThread;

    std::mutex FDMThreadMutex;

    double simTime{0.0};

    void FDMThreadFunction();

private:
    // DATA LOGGING
    std::ofstream FDMLogFile;

    void Log(std::string data);

    void CloseLogFile();

private:
    // UDP COMMUNICATION
    sockaddr_in serverAddr;
    WSADATA wsa;
    SOCKET udpSocket;
};

