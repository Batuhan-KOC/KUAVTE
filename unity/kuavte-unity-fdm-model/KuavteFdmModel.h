#pragma once

#include <winsock2.h>
#include <iostream>
#include <fstream>
#include <string>

#pragma comment(lib,"ws2_32.lib") // Winsock Library
#pragma warning(disable:4996) 

#define BUFLEN 512
#define PORT 8888

#define DLLExport __declspec(dllexport)

struct GymEnvironmentInput{
    unsigned int initialize;
    float throttle;
    float pitch;
    float yaw;
    float roll;
};

class DLLExport KuavteFdmModel{
public:
    KuavteFdmModel();
    ~KuavteFdmModel();

    void Init();
    void Deinit();

    void SendData(unsigned int value);

private:
    // Data Logging
    std::ofstream FDMLogFile;

    void Log(std::string data);

    void CloseLogFile();

    // UDP Communication
    sockaddr_in serverAddr;
    WSADATA wsa;
    SOCKET udpSocket;
};

