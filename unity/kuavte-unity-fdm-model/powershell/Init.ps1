# -----------------------------------------------------------
# Jsbsim download and install
# -----------------------------------------------------------

# Create JSBSim directory
$jsbsimDir = ".\jsbsim"
New-Item -ItemType Directory -Path $jsbsimDir -Force | Out-Null

# Download JSBSim release
$downloadUrl = "https://github.com/JSBSim-Team/jsbsim/archive/refs/tags/v1.2.0.zip"
Invoke-WebRequest -Uri $downloadUrl -OutFile "$jsbsimDir\jsbsim.zip"

# Extract the zip file
Expand-Archive -Path "$jsbsimDir\jsbsim.zip" -DestinationPath $jsbsimDir -Force

# Create build directory
New-Item -ItemType Directory -Path "$jsbsimDir\buildRelease" -Force | Out-Null
New-Item -ItemType Directory -Path "$jsbsimDir\buildDebug" -Force | Out-Null
New-Item -ItemType Directory -Path "$jsbsimDir\installRelease" -Force | Out-Null
New-Item -ItemType Directory -Path "$jsbsimDir\installDebug" -Force | Out-Null

cd "$jsbsimDir"

mkdir lib
cd lib
mkdir debug
mkdir release
cd ..
mkdir include

#-------------------------------------------------------------------------
# CONFIGURING CMAKE
#-------------------------------------------------------------------------

# Navigate to the debug build directory
cd buildDebug

# Run cmake to debug build the project
Write-Host "CONFIGURING CMAKE DEBUG"
cmake -DCMAKE_CONFIGURATION_TYPES:STRING="Debug" -DCMAKE_INSTALL_PREFIX:PATH="../installDebug" -DBUILD_PYTHON_MODULE:BOOL="0" -DBUILD_DOCS:BOOL="0" ../jsbsim-1.2.0

cd ..

# Navigate to the release build directory
cd buildRelease

# Run cmake to release build the project
Write-Host "CONFIGURING CMAKE RELEASE"
cmake -DCMAKE_CONFIGURATION_TYPES:STRING="Release" -DCMAKE_INSTALL_PREFIX:PATH="../installRelease" -DBUILD_PYTHON_MODULE:BOOL="0" -DBUILD_DOCS:BOOL="0" ../jsbsim-1.2.0

cd ..

#-------------------------------------------------------------------------
# BUILDING
#-------------------------------------------------------------------------

# Navigate to the debug build directory
cd buildDebug

Write-Host "CMAKE DEBUG BUILD"
cmake --build . --config Debug

Write-Host "CMAKE DEBUG INSTALL"
cmake --install . --prefix ../installDebug

cd ..

# Navigate to the release build directory
cd buildRelease

Write-Host "CMAKE RELEASE BUILD"
cmake --build . --config Release

Write-Host "CMAKE RELEASE INSTALL"
cmake --install . --prefix ../installRelease

cd ..

#-------------------------------------------------------------------------
# COPY BUILD FILES
#-------------------------------------------------------------------------

Copy-Item -Recurse -Path "installDebug\lib\*" -Destination "lib\debug"
Copy-Item -Recurse -Path "installRelease\lib\*" -Destination "lib\release"
Copy-Item -Recurse -Path "installDebug\include\JSBSim\*" -Destination "include"

# Remove the build directory, downloaded zip file and extracted folder
Remove-Item -Path buildDebug -Recurse -Force
Remove-Item -Path buildRelease -Recurse -Force
Remove-Item -Path installDebug -Recurse -Force
Remove-Item -Path installRelease -Recurse -Force
Remove-Item -Path jsbsim-1.2.0 -Recurse -Force
Remove-Item -Path jsbsim.zip -Force
