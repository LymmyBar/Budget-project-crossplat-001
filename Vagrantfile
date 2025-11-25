Vagrant.configure("2") do |config|
  config.vm.synced_folder ".", "/vagrant", type: "virtualbox"

  config.vm.define "baget" do |baget|
    baget.vm.box = "ubuntu/jammy64"
    baget.vm.hostname = "baget"
    baget.vm.network "private_network", ip: "192.168.56.10"
    baget.vm.provision "shell", path: "deploy/provision/baget.sh"
  end

  config.vm.define "ubuntu" do |ubuntu|
    ubuntu.vm.box = "ubuntu/jammy64"
    ubuntu.vm.hostname = "event-ubuntu"
    ubuntu.vm.network "private_network", ip: "192.168.56.11"
    ubuntu.vm.provision "shell", path: "deploy/provision/ubuntu-client.sh"
  end

  config.vm.define "rocky" do |rocky|
    rocky.vm.box = "generic/rocky9"
    rocky.vm.hostname = "event-rocky"
    rocky.vm.network "private_network", ip: "192.168.56.12"
    rocky.vm.provision "shell", path: "deploy/provision/rocky-client.sh"
  end
end
