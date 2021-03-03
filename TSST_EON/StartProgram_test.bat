start .\CableCloud\bin\Debug\netcoreapp3.1\CableCloud.exe "./Configs/CloudConfig.txt"
timeout 1
start .\Client\bin\Debug\netcoreapp3.1\Client.exe "./Configs/ClientConfig1.txt"

start .\Client\bin\Debug\netcoreapp3.1\Client.exe "./Configs/ClientConfig2.txt"

start .\NCC\bin\Debug\netcoreapp3.1\NCC.exe "./Configs/NCCConfig1.txt" "./Configs/DirectoryConfig1.txt" "./Configs/PolicyConfig1.txt"

start .\NCC\bin\Debug\netcoreapp3.1\NCC.exe "./Configs/NCCConfig2.txt" "./Configs/DirectoryConfig2.txt" "./Configs/PolicyConfig2.txt"

start .\Node\bin\Debug\netcoreapp3.1\Node.exe "./Configs/NodeConfig1.txt"

start .\Node\bin\Debug\netcoreapp3.1\Node.exe "./Configs/NodeConfig2.txt"

start .\Node\bin\Debug\netcoreapp3.1\Node.exe "./Configs/NodeConfig3.txt"

start .\Node\bin\Debug\netcoreapp3.1\Node.exe "./Configs/NodeConfig5.txt"

start .\NetworkManagement\bin\Debug\netcoreapp3.1\NetworkManagement.exe "./Configs/NetworkConfig1.txt" "./Configs/LRM1.txt" "./Configs/RCConfigNode1.txt" "./Configs/RCConfigLink1.txt"

start .\NetworkManagement\bin\Debug\netcoreapp3.1\NetworkManagement.exe "./Configs/NetworkConfig2.txt" "./Configs/LRM2.txt" "./Configs/RCConfigNode2.txt" "./Configs/RCConfigLink2.txt"




