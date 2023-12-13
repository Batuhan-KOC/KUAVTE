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

#define MPH_TO_FPS 1.4666667

#define DLLExport __declspec(dllexport)

struct PositionData{
    float x, y, z;
    float phi, theta, psi;
    float simulation_time;
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
    void StartFDMEnvironment(float frequency = 25, bool windActive = false);

    // Stop FDM Environment
    void StopFDMEnvironment();

    // Send udp position data to the unity environment (x, y, height, psi, phi, theta)
    void SendPosition();

private:
    // FLIGHT DYNAMICS MODEL
    JSBSim::FGFDMExec* fdm{nullptr};

    SGPath modelPath;

    float freq;

    bool windActived{false};
    // LightAir(2mph) - Light Breeze(5.5mph) - Gentle Breeze(10mph) - Moderate Breeze(15.5 mph) - Fresh Breeze(21.5mph) - Strong Breeze(28mph) - Near Gale(35mph) - Gale(42.5mph)
    float windSpeed{0.0};
    // In Radian [Min : 0.0 | Max : 6.283]
    float windDirection{0.0};

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

