@echo off 
taskkill /IM CableCloud.exe /F 
taskkill /IM Client.exe /F 
taskkill /IM NCC.exe /F 
taskkill /IM NetworkManagement.exe /F
taskkill /IM Node.exe /F  
timeout 2 