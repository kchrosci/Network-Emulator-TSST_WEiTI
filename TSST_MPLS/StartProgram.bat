start .\CloudCable\bin\Debug\netcoreapp3.1\CloudCable.exe "./Configs/CloudConfig.txt"
timeout 1
start .\ManagementSystem\bin\Debug\netcoreapp3.1\ManagementSystem.exe "./Configs/mpls_border_config.txt" "./Configs/mpls_config.txt"
timeout 1
start .\NetworkNode\bin\Debug\netcoreapp3.1\NetworkNode.exe "./Configs/NetworkConfig1.txt"
timeout 1
start .\NetworkNode\bin\Debug\netcoreapp3.1\NetworkNode.exe "./Configs/NetworkConfig2.txt"
timeout 1
start .\NetworkNode\bin\Debug\netcoreapp3.1\NetworkNode.exe "./Configs/NetworkConfig3.txt"
timeout 1
start .\NetworkNode\bin\Debug\netcoreapp3.1\NetworkNode.exe "./Configs/NetworkConfig4.txt"
timeout 1
start .\NetworkNode\bin\Debug\netcoreapp3.1\NetworkNode.exe "./Configs/NetworkConfig5.txt"
timeout 1
start .\CustomerNode\bin\Debug\netcoreapp3.1\CustomerNode.exe "./Configs/CustomerConfig1.txt"
timeout 1
start .\CustomerNode\bin\Debug\netcoreapp3.1\CustomerNode.exe "./Configs/CustomerConfig2.txt"
timeout 1
start .\CustomerNode\bin\Debug\netcoreapp3.1\CustomerNode.exe "./Configs/CustomerConfig3.txt"
timeout 1
start .\CustomerNode\bin\Debug\netcoreapp3.1\CustomerNode.exe "./Configs/CustomerConfig4.txt"
timeout 1

