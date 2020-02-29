#!/usr/bin/env bash

# For more information about these steps, see: https://github.com/sqlitebrowser/sqlitebrowser/issues/1229#issuecomment-561395307

# Install necessary packages
sudo apt -y install pcscd libpcsclite1 perl libpcsclite-dev unzip \
    libusb-1.0-0-dev git autoconf automake libtool libsystemd-dev \
    libudev-dev flex qt5-default openssl opensc-pkcs11 libssl-dev \
    pkgconf osslsigncode

function compile_tar {
    tarFile=$1
    baseUrl=$2
    folderName=$3
    
    if [[ ! -f $tarFile ]]; then
        wget $baseUrl/$tarFile
    fi
    rm -rf ./$folderName/
    mkdir $folderName
    tar -xvf $tarFile -C ./$folderName --strip-components=1
    cd $folderName
    ./configure
    make
    sudo make install
    cd ..
    rm -rf ./$folderName/
    # rm -f $tarFile
}

function install_pcsc_lite {
    rm -rf ./PCSC/
    git clone https://salsa.debian.org/rousseau/PCSC.git
    cd PCSC
    ./bootstrap
    ./configure
    make
    sudo make install
    cd ..
    rm -rf ./PCSC/
}

function install_pcsc_perl {
    tarFile="pcsc-perl-1.4.14.tar.bz2"
    if [[ ! -f $tarFile ]]; then
        wget http://ludovic.rousseau.free.fr/softwares/pcsc-perl/$tarFile
    fi
    rm -rf ./pcsc-perl/
    mkdir pcsc-perl
    tar -xvf $tarFile -C ./pcsc-perl --strip-components=1
    cd pcsc-perl
    # sed -i -- 's=pcsclite=PCSC/pcsclite=g' *.h
    perl ./Makefile.PL
    make
    sudo make install
    cd ..
    rm -rf ./pcsc-perl/
    # rm -f $tarFile
}

function install_pcsc_tools {
    compile_tar "pcsc-tools-1.5.5.tar.bz2" "http://ludovic.rousseau.free.fr/softwares/pcsc-tools" "pcsc-tools"
}

function install_card_driver {
    baseName="ACS-Unified-Driver-Lnx-Mac-117-P"
    zipFile="$baseName.zip"
    if [[ ! -f $zipFile ]]; then
        wget https://www.acs.com.hk/download-driver-unified/11566/$zipFile
    fi
    rm -rf ./card-drivers/
    mkdir card-drivers
    unzip $zipFile -d ./card-drivers
    cd card-drivers/$baseName
    mkdir acsccid
    tar -xvf acsccid-1.1.7.tar.bz2 -C ./acsccid --strip-components=1
    cd acsccid
    ./configure
    make
    sudo make install
    cd ../../..
    rm -rf ./card-drivers/
    # rm -f $zipFile
}

function install_libp11 {
    compile_tar "libp11-0.4.10.tar.gz" "https://github.com/OpenSC/libp11/releases/download/libp11-0.4.10" "libp11"
}

function install_card_manager {
    binFile="proCertumCardManager-1_3_4-x86_64-glibc2_23-qt5_2_1.bin"
    if [[ ! -f $binFile ]]; then
        wget https://www.certum.pl/pl/upload_module/wysiwyg/software/CardManager/Linux/$binFile
    fi
    chmod +x $binFile
    ./$binFile --nox11 <<<$'tak\ntak\n'
    # rm -f $binFile
}

function install_dotnet_core {
    debFile="packages-microsoft-prod.deb"
    if [[ ! -f $debFile ]]; then
        wget -q https://packages.microsoft.com/config/ubuntu/19.04/$debFile -O $debFile
    fi
    sudo dpkg -i $debFile
    sudo apt-get update
    sudo apt-get install -y apt-transport-https
    sudo apt-get update
    sudo apt-get install -y dotnet-sdk-3.1
}

function install_pwsh {
    # sudo dotnet tool install --global PowerShell
    sudo snap install powershell --classic
}

function install_azure_agent {
    if [ -z "$PAT" ]; then
        echo ERROR: Need to define PAT variable to install the Azure Pipelines agent
        return
    fi

    tarFile="vsts-agent-linux-x64-2.165.0.tar.gz"
    if [[ ! -f $tarFile ]]; then
        wget https://vstsagentpackage.azureedge.net/agent/2.165.0/$tarFile
    fi
    mkdir devops-agent
    cd devops-agent
    tar zxvf ../$tarFile
    sudo ./bin/installdependencies.sh

    # This is in case it was run previously
    sudo ./svc.sh uninstall
    ./config.sh remove --unattended --auth pat --token $PAT

    ./config.sh --unattended --acceptTeeEula --url https://dev.azure.com/fernandreu-public --auth pat --token $PAT --pool default --agent fernando-VirtualBox --replace

    sudo ./svc.sh install
    sudo ./svc.sh start
}

install_pcsc_lite
install_pcsc_perl
install_pcsc_tools
install_card_driver
install_card_manager
install_libp11

# This is not necessary right now due to the way PS is installed, but it might make
# dotnet builds faster if the agent is ever used for that
# install_dotnet_core

install_pwsh
install_azure_agent

echo REMEMBER to put the public key certificate in: $HOME/codesign.spc
