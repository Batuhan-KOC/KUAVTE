cmake_minimum_required(VERSION 3.15.0)
project(KuavteFdmModel VERSION 0.1.0 LANGUAGES C CXX)

set(CMAKE_CXX_STANDARD 11)

add_definitions(-DEXPORT_KUAVTE)

set(SOURCE_FILES KuavteFdmModel.cpp)

add_library(KuavteFdmModel SHARED ${SOURCE_FILES})

# Output the shared library to the Unity project's Asset folder
set_target_properties(KuavteFdmModel PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY_DEBUG ${CMAKE_CURRENT_SOURCE_DIR}/../kuavte-unity/Assets/kuavte-fdm-model
    RUNTIME_OUTPUT_DIRECTORY_RELEASE ${CMAKE_CURRENT_SOURCE_DIR}/../kuavte-unity/Assets/kuavte-fdm-model
)

target_link_libraries(KuavteFdmModel PRIVATE ws2_32 wsock32)