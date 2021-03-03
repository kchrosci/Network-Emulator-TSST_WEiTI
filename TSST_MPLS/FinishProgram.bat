@echo off 
taskkill /IM NetworkNode.exe /F 
taskkill /IM CloudCable.exe /F 
taskkill /IM ManagementSystem.exe /F 
taskkill /IM CustomerNode.exe /F 
timeout 10 