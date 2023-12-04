#pragma once

#include <iostream>

#define DLLExport __declspec(dllexport)

class DLLExport KuavteFdmModel{
public:
    KuavteFdmModel();
    ~KuavteFdmModel();

    void DoSomething();
private:
};

