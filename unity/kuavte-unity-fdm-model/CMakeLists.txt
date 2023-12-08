cmake_minimum_required(VERSION 3.15.0)
project(KuavteFdmModel VERSION 0.1.0 LANGUAGES C CXX)

set(CMAKE_CXX_STANDARD 11)

add_compile_definitions(EXPORT_KUAVTE JSBSIM_STATIC_LINK)

set(SOURCE_FILES KuavteFdmModel.cpp)

add_library(KuavteFdmModel SHARED ${SOURCE_FILES})

target_include_directories(KuavteFdmModel PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/jsbsim/include)

# Output the shared library to the Unity project's Asset folder
set_target_properties(KuavteFdmModel PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY_DEBUG ${CMAKE_CURRENT_SOURCE_DIR}/../kuavte-unity/Assets/kuavte-fdm-model
    RUNTIME_OUTPUT_DIRECTORY_RELEASE ${CMAKE_CURRENT_SOURCE_DIR}/../kuavte-unity/Assets/kuavte-fdm-model
)

add_library(JSBSIM_LIB IMPORTED UNKNOWN)
set_target_properties(JSBSIM_LIB PROPERTIES
  IMPORTED_LOCATION_RELEASE
    ${CMAKE_CURRENT_SOURCE_DIR}/jsbsim/lib/release/JSBSim.lib
  IMPORTED_LOCATION_DEBUG
    ${CMAKE_CURRENT_SOURCE_DIR}/jsbsim/lib/debug/JSBSim.lib
)

set_target_properties(JSBSIM_LIB PROPERTIES
  # For Debug version of the project use DEBUG-suffixed library location
  MAP_IMPORTED_CONFIG_DEBUG DEBUG
  # For Release version of the project use RELEASE-suffixed library location
  MAP_IMPORTED_CONFIG_RELEASE RELEASE
  # For ReleaseWithDebInfo version of the project use DEBUG-suffixed library location
  MAP_IMPORTED_CONFIG_RELWITHDEBINFO DEBUG
  # For MinSizeRel version of the project use RELEASE-suffixed library location
  MAP_IMPORTED_CONFIG_MINSIZEREL RELEASE
)

target_link_libraries(KuavteFdmModel PRIVATE JSBSIM_LIB ws2_32 wsock32)