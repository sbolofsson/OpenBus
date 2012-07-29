@echo off

REM Set context to vs tools
IF EXIST "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" (
	CALL "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat"
)

REM Clean up solar store
certmgr -del -all -s -r LocalMachine Solar

REM Clean up trusted people store
certmgr -del -c -n "Database" -s -r LocalMachine TrustedPeople
certmgr -del -c -n "OPC" -s -r LocalMachine TrustedPeople
certmgr -del -c -n "Bus" -s -r LocalMachine TrustedPeople
certmgr -del -c -n "Weather" -s -r LocalMachine TrustedPeople
certmgr -del -c -n "Calendar" -s -r LocalMachine TrustedPeople
certmgr -del -c -n "IHC" -s -r LocalMachine TrustedPeople
certmgr -del -c -n "AppService" -s -r LocalMachine TrustedPeople
certmgr -del -c -n "CLUE" -s -r LocalMachine TrustedPeople

REM Add new certificates
makecert -len 2048 -sr LocalMachine -pe -n "CN=Solar CA;OU=Solar;O=DTU;C=DK;" -cy authority -r -ss Solar Solar.cer
makecert -len 2048 -sr LocalMachine -pe -n "CN=Database;OU=Solar;O=DTU;C=DK;" -is Solar -in "Solar CA" -ss TrustedPeople -sky exchange Database.cer
makecert -len 2048 -sr LocalMachine -pe -n "CN=OPC;OU=Solar;O=DTU;C=DK;" -is Solar -in "Solar CA" -ss TrustedPeople -sky exchange OPC.cer
makecert -len 2048 -sr LocalMachine -pe -n "CN=Bus;OU=Solar;O=DTU;C=DK;" -is Solar -in "Solar CA" -ss TrustedPeople -sky exchange Bus.cer
makecert -len 2048 -sr LocalMachine -pe -n "CN=Weather;OU=Solar;O=DTU;C=DK;" -is Solar -in "Solar CA" -ss TrustedPeople -sky exchange Weather.cer
makecert -len 2048 -sr LocalMachine -pe -n "CN=Calendar;OU=Solar;O=DTU;C=DK;" -is Solar -in "Solar CA" -ss TrustedPeople -sky exchange Calendar.cer
makecert -len 2048 -sr LocalMachine -pe -n "CN=IHC;OU=Solar;O=DTU;C=DK;" -is Solar -in "Solar CA" -ss TrustedPeople -sky exchange IHC.cer
makecert -len 2048 -sr LocalMachine -pe -n "CN=DBTester;OU=Solar;O=DTU;C=DK;" -is Solar -in "Solar CA" -ss TrustedPeople -sky exchange DatabaseTester.cer
makecert -len 2048 -sr LocalMachine -pe -n "CN=CLUE;OU=Solar;O=DTU;C=DK;" -is Solar -in "Solar CA" -ss TrustedPeople -sky exchange CLUE.cer
makecert -len 2048 -sr LocalMachine -pe -n "CN=BCPM;OU=Solar;O=DTU;C=DK;" -is Solar -in "Solar CA" -ss TrustedPeople -sky exchange BCPM.cer

REM Add new IIS certificates
makecert -r -pe -n "CN=AppService;OU=Solar;O=DTU;C=DK;" -b 01/01/2000 -e 01/01/2040 -eku 1.3.6.1.5.5.7.3.1 -ss my -sr LocalMachine -sky exchange -sp "Microsoft RSA SChannel Cryptographic Provider" -sy 12

pause