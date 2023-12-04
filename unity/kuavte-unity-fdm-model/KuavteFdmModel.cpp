#include "KuavteFdmModel.h"

KuavteFdmModel::KuavteFdmModel()
{

}

KuavteFdmModel::~KuavteFdmModel()
{
    
}

void KuavteFdmModel::DoSomething()
{
    
}

extern "C" DLLExport KuavteFdmModel* CreateFdmModel(){
    return new KuavteFdmModel();
}

extern "C" DLLExport void DeleteFdmModel(KuavteFdmModel* model){
    if(model != NULL){
        delete model;
        model = NULL;
    }
}

extern "C" DLLExport void DoSomething(KuavteFdmModel* model){
    if(model != NULL){
        model->DoSomething();
    }
}
