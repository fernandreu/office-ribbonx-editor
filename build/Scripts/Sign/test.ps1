. ./remote-sign.ps1
$CODESIGN_PIN = "13910610"
$CODESIGN_HOST = "fernando@fernandreu.ddns.net"
$CODESIGN_PORT = "22025"
Update-AllFiles "C:\Users\FernA\Downloads\test" -HostName $CODESIGN_HOST -Pin $CODESIGN_PIN -Port $CODESIGN_PORT
