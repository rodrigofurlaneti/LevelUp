O Windows PowerShell
Copyright (C) Microsoft Corporation. Todos os direitos reservados.

Instale o PowerShell mais recente para obter novos recursos e aprimoramentos! https://aka.ms/PSWindows

PS C:\WINDOWS\system32> ssh-keygen -t ed25519 -C "rodrigo@furlaneti.com"
Generating public/private ed25519 key pair.
Enter file in which to save the key (C:\Users\adm/.ssh/id_ed25519):
C:\Users\adm/.ssh/id_ed25519 already exists.
Overwrite (y/n)? y
Enter passphrase (empty for no passphrase):
Enter same passphrase again:
Your identification has been saved in C:\Users\adm/.ssh/id_ed25519
Your public key has been saved in C:\Users\adm/.ssh/id_ed25519.pub
The key fingerprint is:
SHA256:C2FbIE3Qpjw/wsMiay7RQig/zyilxWPs7WWOOyrBC9E rodrigo@furlaneti.com
The key's randomart image is:
+--[ED25519 256]--+
|    o=o          |
|     .+.         |
|.. . oo .        |
|+.E +. +         |
|+* o oo S        |
|*.% = o. .       |
|.%.O oo..        |
|*oo ==           |
|++.o+o.          |
+----[SHA256]-----+
PS C:\WINDOWS\system32> Get-Content $env:adm\.ssh\id_ed25519.pub | Set-Clipboard
>>
Get-Content : Não é possível localizar o caminho 'C:\.ssh\id_ed25519.pub' porque ele não existe.
No linha:1 caractere:1
+ Get-Content $env:adm\.ssh\id_ed25519.pub | Set-Clipboard
+ ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    + CategoryInfo          : ObjectNotFound: (C:\.ssh\id_ed25519.pub:String) [Get-Content], ItemNotFoundException
    + FullyQualifiedErrorId : PathNotFound,Microsoft.PowerShell.Commands.GetContentCommand

PS C:\WINDOWS\system32> Get-Content C:\Users\adm\.ssh\id_ed25519.pub | Set-Clipboard
>>
PS C:\WINDOWS\system32> ssh root@191.252.103.107
The authenticity of host '191.252.103.107 (191.252.103.107)' can't be established.
ED25519 key fingerprint is SHA256:t8D1VmCoBI/pDnili4X45xKqnTte5hKZZ5nU/ixboGk.
This key is not known by any other names.
Are you sure you want to continue connecting (yes/no/[fingerprint])? yes
Warning: Permanently added '191.252.103.107' (ED25519) to the list of known hosts.
root@191.252.103.107's password:
Connection closed by 191.252.103.107 port 22
PS C:\WINDOWS\system32> ssh root@191.252.103.107
root@191.252.103.107's password:
         _,met$$$$$gg.           root@vps61268
      ,g$$$$$$$$$$$$$$$P.        OS: Debian 12 bookworm
    ,g$$P""       """Y$$.".      Kernel: x86_64 Linux 6.1.0-38-amd64
   ,$$P'              `$$$.      Uptime: 37m
  ',$$P       ,ggs.     `$$b:    Packages: 453
  `d$$'     ,$P"'   .    $$$     Shell: dash
   $$P      d$'     ,    $$P     Disk: 2.5G / 19G (14%)
   $$:      $$.   -    ,d$$'     CPU: Intel Xeon Silver 4316 @ 2.295GHz
   $$\;      Y$b._   _,d$P'      GPU: Device 1234:1111
   Y$$.    `.`"Y$$$$P"'          RAM: 169MiB / 414MiB
   `$$b      "-.__
    `Y$$
     `Y$$.
       `$$b.
         `Y$$b.
            `"Y$b._
                `""""

Linux vps61268 6.1.0-38-amd64 #1 SMP PREEMPT_DYNAMIC Debian 6.1.147-1 (2025-08-02) x86_64

The programs included with the Debian GNU/Linux system are free software;
the exact distribution terms for each program are described in the
individual files in /usr/share/doc/*/copyright.

Debian GNU/Linux comes with ABSOLUTELY NO WARRANTY, to the extent
permitted by applicable law.
Last login: Sun Sep  7 09:58:51 2025
root@vps61268:~# mkdir -p /root/.ssh
root@vps61268:~# chmod 700 /root/.ssh
root@vps61268:~# nano /root/.ssh/authorized_keys
root@vps61268:~# chmod 600 /root/.ssh/authorized_keys
root@vps61268:~# ^[[200~nano /etc/ssh/sshd_config
-bash: $'\E[200~nano': command not found
root@vps61268:~# ~nano /etc/ssh/sshd_config
-bash: ~nano: command not found
root@vps61268:~# nano /etc/ssh/sshd_config
root@vps61268:~# systemctl reload ssh
root@vps61268:~# ufw default deny incoming
Default incoming policy changed to 'deny'
(be sure to update your rules accordingly)
root@vps61268:~# ufw default allow outgoing
Default outgoing policy changed to 'allow'
(be sure to update your rules accordingly)
root@vps61268:~# ufw allow OpenSSH
Rules updated
Rules updated (v6)
root@vps61268:~# ufw allow 80/tcp
Rules updated
Rules updated (v6)
root@vps61268:~# ufw allow 443/tcp
Rules updated
Rules updated (v6)
root@vps61268:~# ufw allow 1433/tcp
Rules updated
Rules updated (v6)
root@vps61268:~# ufw enable
Command may disrupt existing ssh connections. Proceed with operation (y|n)? y
Firewall is active and enabled on system startup
root@vps61268:~# ufw status verbose
Status: active
Logging: on (low)
Default: deny (incoming), allow (outgoing), disabled (routed)
New profiles: skip

To                         Action      From
--                         ------      ----
22/tcp (OpenSSH)           ALLOW IN    Anywhere
80/tcp                     ALLOW IN    Anywhere
443/tcp                    ALLOW IN    Anywhere
1433/tcp                   ALLOW IN    Anywhere
22/tcp (OpenSSH (v6))      ALLOW IN    Anywhere (v6)
80/tcp (v6)                ALLOW IN    Anywhere (v6)
443/tcp (v6)               ALLOW IN    Anywhere (v6)
1433/tcp (v6)              ALLOW IN    Anywhere (v6)

root@vps61268:~# ^[[200~apt update && apt upgrade -y
-bash: $'\E[200~apt': command not found
root@vps61268:~# apt install -y ca-certificates curl gnupg lsb-release
Reading package lists... Done
Building dependency tree... Done
Reading state information... Done
ca-certificates is already the newest version (20230311+deb12u1).
curl is already the newest version (7.88.1-10+deb12u14).
gnupg is already the newest version (2.2.40-1.1+deb12u1).
lsb-release is already the newest version (12.0-1).
0 upgraded, 0 newly installed, 0 to remove and 0 not upgraded.
root@vps61268:~# ~apt update && apt upgrade -y
-bash: ~apt: command not found
root@vps61268:~# apt install -y ca-certificates curl gnupg lsb-release
Reading package lists... Done
Building dependency tree... Done
Reading state information... Done
ca-certificates is already the newest version (20230311+deb12u1).
curl is already the newest version (7.88.1-10+deb12u14).
gnupg is already the newest version (2.2.40-1.1+deb12u1).
lsb-release is already the newest version (12.0-1).
0 upgraded, 0 newly installed, 0 to remove and 0 not upgraded.
root@vps61268:~# apt update && apt upgrade -y
Hit:1 http://debian.mirror.locaweb.com.br bookworm InRelease
Hit:2 http://debian.mirror.locaweb.com.br bookworm-updates InRelease
Hit:3 http://security.debian.org/debian-security bookworm-security InRelease
Reading package lists... Done
Building dependency tree... Done
Reading state information... Done
All packages are up to date.
Reading package lists... Done
Building dependency tree... Done
Reading state information... Done
Calculating upgrade... Done
0 upgraded, 0 newly installed, 0 to remove and 0 not upgraded.
root@vps61268:~# apt install -y ca-certificates curl gnupg lsb-release
apt install -y ca-certificates curl gnupg lsb-release
Reading package lists... Done
Building dependency tree... Done
Reading state information... Done
ca-certificates is already the newest version (20230311+deb12u1).
curl is already the newest version (7.88.1-10+deb12u14).
gnupg is already the newest version (2.2.40-1.1+deb12u1).
lsb-release is already the newest version (12.0-1).
0 upgraded, 0 newly installed, 0 to remove and 0 not upgraded.
Reading package lists... Done
Building dependency tree... Done
Reading state information... Done
ca-certificates is already the newest version (20230311+deb12u1).
curl is already the newest version (7.88.1-10+deb12u14).
gnupg is already the newest version (2.2.40-1.1+deb12u1).
lsb-release is already the newest version (12.0-1).
0 upgraded, 0 newly installed, 0 to remove and 0 not upgraded.
root@vps61268:~# mkdir -p /etc/apt/keyrings
root@vps61268:~# curl -fsSL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o /etc/apt/keyrings/microsoft.gpg
File '/etc/apt/keyrings/microsoft.gpg' exists. Overwrite? (y/N) y
root@vps61268:~# echo "deb [arch=amd64,arm64,armhf signed-by=/etc/apt/keyrings/microsoft.gpg] https://packages.microsoft.com/debian/12/prod bookworm main" > /etc/apt/sources.list.d/microsoft-prod.list
root@vps61268:~# apt update
Hit:1 http://debian.mirror.locaweb.com.br bookworm InRelease
Hit:2 http://debian.mirror.locaweb.com.br bookworm-updates InRelease
Hit:3 http://security.debian.org/debian-security bookworm-security InRelease
Get:4 https://packages.microsoft.com/debian/12/prod bookworm InRelease [3,618 B]
Get:5 https://packages.microsoft.com/debian/12/prod bookworm/main armhf Packages [16.5 kB]
Get:6 https://packages.microsoft.com/debian/12/prod bookworm/main amd64 Packages [111 kB]
Get:7 https://packages.microsoft.com/debian/12/prod bookworm/main arm64 Packages [32.3 kB]
Get:8 https://packages.microsoft.com/debian/12/prod bookworm/main all Packages [573 B]
Fetched 164 kB in 1s (314 kB/s)
Reading package lists... Done
Building dependency tree... Done
Reading state information... Done
All packages are up to date.
root@vps61268:~# apt install -y dotnet-sdk-8.0 aspnetcore-runtime-8.0
Reading package lists... Done
Building dependency tree... Done
Reading state information... Done
The following additional packages will be installed:
  aspnetcore-targeting-pack-8.0 dotnet-apphost-pack-8.0 dotnet-host dotnet-hostfxr-8.0 dotnet-runtime-8.0 dotnet-runtime-deps-8.0 dotnet-targeting-pack-8.0
  netstandard-targeting-pack-2.1
The following NEW packages will be installed:
  aspnetcore-runtime-8.0 aspnetcore-targeting-pack-8.0 dotnet-apphost-pack-8.0 dotnet-host dotnet-hostfxr-8.0 dotnet-runtime-8.0 dotnet-runtime-deps-8.0
  dotnet-sdk-8.0 dotnet-targeting-pack-8.0 netstandard-targeting-pack-2.1
0 upgraded, 10 newly installed, 0 to remove and 0 not upgraded.
Need to get 143 MB of archives.
After this operation, 589 MB of additional disk space will be used.
Get:1 https://packages.microsoft.com/debian/12/prod bookworm/main amd64 dotnet-host amd64 9.0.8-1 [36.9 kB]
Get:2 https://packages.microsoft.com/debian/12/prod bookworm/main amd64 dotnet-hostfxr-8.0 amd64 8.0.19-1 [108 kB]
Get:3 https://packages.microsoft.com/debian/12/prod bookworm/main amd64 dotnet-runtime-deps-8.0 amd64 8.0.19-1 [2,894 B]
Get:4 https://packages.microsoft.com/debian/12/prod bookworm/main amd64 dotnet-runtime-8.0 amd64 8.0.19-1 [23.1 MB]
Get:5 https://packages.microsoft.com/debian/12/prod bookworm/main amd64 aspnetcore-runtime-8.0 amd64 8.0.19-1 [7,719 kB]
Get:6 https://packages.microsoft.com/debian/12/prod bookworm/main amd64 dotnet-targeting-pack-8.0 amd64 8.0.19-1 [2,800 kB]
Get:7 https://packages.microsoft.com/debian/12/prod bookworm/main amd64 aspnetcore-targeting-pack-8.0 amd64 8.0.19-1 [1,944 kB]
Get:8 https://packages.microsoft.com/debian/12/prod bookworm/main amd64 dotnet-apphost-pack-8.0 amd64 8.0.19-1 [3,519 kB]
Get:9 https://packages.microsoft.com/debian/12/prod bookworm/main amd64 netstandard-targeting-pack-2.1 amd64 2.1.0-1 [1,476 kB]
Get:10 https://packages.microsoft.com/debian/12/prod bookworm/main amd64 dotnet-sdk-8.0 amd64 8.0.413-1 [102 MB]
Fetched 143 MB in 23s (6,229 kB/s)
Selecting previously unselected package dotnet-host.
(Reading database ... 34483 files and directories currently installed.)
Preparing to unpack .../0-dotnet-host_9.0.8-1_amd64.deb ...
Unpacking dotnet-host (9.0.8-1) ...
Selecting previously unselected package dotnet-hostfxr-8.0.
Preparing to unpack .../1-dotnet-hostfxr-8.0_8.0.19-1_amd64.deb ...
Unpacking dotnet-hostfxr-8.0 (8.0.19-1) ...
Selecting previously unselected package dotnet-runtime-deps-8.0.
Preparing to unpack .../2-dotnet-runtime-deps-8.0_8.0.19-1_amd64.deb ...
Unpacking dotnet-runtime-deps-8.0 (8.0.19-1) ...
Selecting previously unselected package dotnet-runtime-8.0.
Preparing to unpack .../3-dotnet-runtime-8.0_8.0.19-1_amd64.deb ...
Unpacking dotnet-runtime-8.0 (8.0.19-1) ...
Selecting previously unselected package aspnetcore-runtime-8.0.
Preparing to unpack .../4-aspnetcore-runtime-8.0_8.0.19-1_amd64.deb ...
Unpacking aspnetcore-runtime-8.0 (8.0.19-1) ...
Selecting previously unselected package dotnet-targeting-pack-8.0.
Preparing to unpack .../5-dotnet-targeting-pack-8.0_8.0.19-1_amd64.deb ...
Unpacking dotnet-targeting-pack-8.0 (8.0.19-1) ...
Selecting previously unselected package aspnetcore-targeting-pack-8.0.
Preparing to unpack .../6-aspnetcore-targeting-pack-8.0_8.0.19-1_amd64.deb ...
Unpacking aspnetcore-targeting-pack-8.0 (8.0.19-1) ...
Selecting previously unselected package dotnet-apphost-pack-8.0.
Preparing to unpack .../7-dotnet-apphost-pack-8.0_8.0.19-1_amd64.deb ...
Unpacking dotnet-apphost-pack-8.0 (8.0.19-1) ...
Selecting previously unselected package netstandard-targeting-pack-2.1.
Preparing to unpack .../8-netstandard-targeting-pack-2.1_2.1.0-1_amd64.deb ...
Unpacking netstandard-targeting-pack-2.1 (2.1.0-1) ...
Selecting previously unselected package dotnet-sdk-8.0.
Preparing to unpack .../9-dotnet-sdk-8.0_8.0.413-1_amd64.deb ...
Unpacking dotnet-sdk-8.0 (8.0.413-1) ...
Setting up dotnet-host (9.0.8-1) ...
Setting up dotnet-targeting-pack-8.0 (8.0.19-1) ...
Setting up netstandard-targeting-pack-2.1 (2.1.0-1) ...
Setting up aspnetcore-targeting-pack-8.0 (8.0.19-1) ...
Setting up dotnet-apphost-pack-8.0 (8.0.19-1) ...
Setting up dotnet-runtime-deps-8.0 (8.0.19-1) ...
Setting up dotnet-hostfxr-8.0 (8.0.19-1) ...
Setting up dotnet-runtime-8.0 (8.0.19-1) ...
Setting up aspnetcore-runtime-8.0 (8.0.19-1) ...
Setting up dotnet-sdk-8.0 (8.0.413-1) ...
root@vps61268:~# curl -fsSL https://download.docker.com/linux/debian/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg
root@vps61268:~# chmod a+r /etc/apt/keyrings/docker.gpg
root@vps61268:~# echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/debian bookworm stable" > /etc/apt/sources.list.d/docker.list
echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/debian bookworm stable" > /etc/apt/sources.list.d/docker.list
root@vps61268:~# apt update
Hit:1 http://debian.mirror.locaweb.com.br bookworm InRelease
Hit:2 http://debian.mirror.locaweb.com.br bookworm-updates InRelease
Hit:3 http://security.debian.org/debian-security bookworm-security InRelease
Get:4 https://download.docker.com/linux/debian bookworm InRelease [47.0 kB]
Hit:5 https://packages.microsoft.com/debian/12/prod bookworm InRelease
Get:6 https://download.docker.com/linux/debian bookworm/stable amd64 Packages [46.2 kB]
Fetched 93.2 kB in 0s (219 kB/s)
Reading package lists... Done
Building dependency tree... Done
Reading state information... Done
All packages are up to date.
root@vps61268:~# apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
Reading package lists... Done
Building dependency tree... Done
Reading state information... Done
The following additional packages will be installed:
  docker-ce-rootless-extras git git-man liberror-perl libgdbm-compat4 libgdbm6 libperl5.36 libslirp0 patch perl perl-modules-5.36 pigz slirp4netns
Suggested packages:
  cgroupfs-mount | cgroup-lite docker-model-plugin git-daemon-run | git-daemon-sysvinit git-doc git-email git-gui gitk gitweb git-cvs git-mediawiki git-svn gdbm-l10n
  ed diffutils-doc perl-doc libterm-readline-gnu-perl | libterm-readline-perl-perl make libtap-harness-archive-perl
The following NEW packages will be installed:
  containerd.io docker-buildx-plugin docker-ce docker-ce-cli docker-ce-rootless-extras docker-compose-plugin git git-man liberror-perl libgdbm-compat4 libgdbm6
  libperl5.36 libslirp0 patch perl perl-modules-5.36 pigz slirp4netns
0 upgraded, 18 newly installed, 0 to remove and 0 not upgraded.
Need to get 120 MB of archives.
After this operation, 528 MB of additional disk space will be used.
Get:1 http://debian.mirror.locaweb.com.br bookworm/main amd64 perl-modules-5.36 all 5.36.0-7+deb12u3 [2,815 kB]
Get:2 http://debian.mirror.locaweb.com.br bookworm/main amd64 libgdbm6 amd64 1.23-3 [72.2 kB]
Get:3 http://debian.mirror.locaweb.com.br bookworm/main amd64 libgdbm-compat4 amd64 1.23-3 [48.2 kB]
Get:4 http://debian.mirror.locaweb.com.br bookworm/main amd64 libperl5.36 amd64 5.36.0-7+deb12u3 [4,196 kB]
Get:5 https://download.docker.com/linux/debian bookworm/stable amd64 containerd.io amd64 1.7.27-1 [30.5 MB]
Get:6 http://debian.mirror.locaweb.com.br bookworm/main amd64 perl amd64 5.36.0-7+deb12u3 [239 kB]
Get:7 http://debian.mirror.locaweb.com.br bookworm/main amd64 pigz amd64 2.6-1 [64.0 kB]
Get:8 http://debian.mirror.locaweb.com.br bookworm/main amd64 liberror-perl all 0.17029-2 [29.0 kB]
Get:9 http://debian.mirror.locaweb.com.br bookworm/main amd64 git-man all 1:2.39.5-0+deb12u2 [2,053 kB]
Get:10 http://debian.mirror.locaweb.com.br bookworm/main amd64 git amd64 1:2.39.5-0+deb12u2 [7,260 kB]
Get:11 http://debian.mirror.locaweb.com.br bookworm/main amd64 libslirp0 amd64 4.7.0-1 [63.0 kB]
Get:12 http://debian.mirror.locaweb.com.br bookworm/main amd64 patch amd64 2.7.6-7 [128 kB]
Get:13 http://debian.mirror.locaweb.com.br bookworm/main amd64 slirp4netns amd64 1.2.0-1 [37.5 kB]
Get:14 https://download.docker.com/linux/debian bookworm/stable amd64 docker-ce-cli amd64 5:28.4.0-1~debian.12~bookworm [16.5 MB]
Get:15 https://download.docker.com/linux/debian bookworm/stable amd64 docker-ce amd64 5:28.4.0-1~debian.12~bookworm [19.7 MB]
Get:16 https://download.docker.com/linux/debian bookworm/stable amd64 docker-buildx-plugin amd64 0.27.0-1~debian.12~bookworm [15.9 MB]
Get:17 https://download.docker.com/linux/debian bookworm/stable amd64 docker-ce-rootless-extras amd64 5:28.4.0-1~debian.12~bookworm [6,479 kB]
Get:18 https://download.docker.com/linux/debian bookworm/stable amd64 docker-compose-plugin amd64 2.39.2-1~debian.12~bookworm [14.2 MB]
Fetched 120 MB in 7s (18.5 MB/s)
Selecting previously unselected package perl-modules-5.36.
(Reading database ... 40069 files and directories currently installed.)
Preparing to unpack .../00-perl-modules-5.36_5.36.0-7+deb12u3_all.deb ...
Unpacking perl-modules-5.36 (5.36.0-7+deb12u3) ...
Selecting previously unselected package libgdbm6:amd64.
Preparing to unpack .../01-libgdbm6_1.23-3_amd64.deb ...
Unpacking libgdbm6:amd64 (1.23-3) ...
Selecting previously unselected package libgdbm-compat4:amd64.
Preparing to unpack .../02-libgdbm-compat4_1.23-3_amd64.deb ...
Unpacking libgdbm-compat4:amd64 (1.23-3) ...
Selecting previously unselected package libperl5.36:amd64.
Preparing to unpack .../03-libperl5.36_5.36.0-7+deb12u3_amd64.deb ...
Unpacking libperl5.36:amd64 (5.36.0-7+deb12u3) ...
Selecting previously unselected package perl.
Preparing to unpack .../04-perl_5.36.0-7+deb12u3_amd64.deb ...
Unpacking perl (5.36.0-7+deb12u3) ...
Selecting previously unselected package containerd.io.
Preparing to unpack .../05-containerd.io_1.7.27-1_amd64.deb ...
Unpacking containerd.io (1.7.27-1) ...
Selecting previously unselected package docker-ce-cli.
Preparing to unpack .../06-docker-ce-cli_5%3a28.4.0-1~debian.12~bookworm_amd64.deb ...
Unpacking docker-ce-cli (5:28.4.0-1~debian.12~bookworm) ...
Selecting previously unselected package docker-ce.
Preparing to unpack .../07-docker-ce_5%3a28.4.0-1~debian.12~bookworm_amd64.deb ...
Unpacking docker-ce (5:28.4.0-1~debian.12~bookworm) ...
Selecting previously unselected package pigz.
Preparing to unpack .../08-pigz_2.6-1_amd64.deb ...
Unpacking pigz (2.6-1) ...
Selecting previously unselected package docker-buildx-plugin.
Preparing to unpack .../09-docker-buildx-plugin_0.27.0-1~debian.12~bookworm_amd64.deb ...
Unpacking docker-buildx-plugin (0.27.0-1~debian.12~bookworm) ...
Selecting previously unselected package docker-ce-rootless-extras.
Preparing to unpack .../10-docker-ce-rootless-extras_5%3a28.4.0-1~debian.12~bookworm_amd64.deb ...
Unpacking docker-ce-rootless-extras (5:28.4.0-1~debian.12~bookworm) ...
Selecting previously unselected package docker-compose-plugin.
Preparing to unpack .../11-docker-compose-plugin_2.39.2-1~debian.12~bookworm_amd64.deb ...
Unpacking docker-compose-plugin (2.39.2-1~debian.12~bookworm) ...
Selecting previously unselected package liberror-perl.
Preparing to unpack .../12-liberror-perl_0.17029-2_all.deb ...
Unpacking liberror-perl (0.17029-2) ...
Selecting previously unselected package git-man.
Preparing to unpack .../13-git-man_1%3a2.39.5-0+deb12u2_all.deb ...
Unpacking git-man (1:2.39.5-0+deb12u2) ...
Selecting previously unselected package git.
Preparing to unpack .../14-git_1%3a2.39.5-0+deb12u2_amd64.deb ...
Unpacking git (1:2.39.5-0+deb12u2) ...
Selecting previously unselected package libslirp0:amd64.
Preparing to unpack .../15-libslirp0_4.7.0-1_amd64.deb ...
Unpacking libslirp0:amd64 (4.7.0-1) ...
Selecting previously unselected package patch.
Preparing to unpack .../16-patch_2.7.6-7_amd64.deb ...
Unpacking patch (2.7.6-7) ...
Selecting previously unselected package slirp4netns.
Preparing to unpack .../17-slirp4netns_1.2.0-1_amd64.deb ...
Unpacking slirp4netns (1.2.0-1) ...
Setting up docker-buildx-plugin (0.27.0-1~debian.12~bookworm) ...
Setting up perl-modules-5.36 (5.36.0-7+deb12u3) ...
Setting up containerd.io (1.7.27-1) ...
Created symlink /etc/systemd/system/multi-user.target.wants/containerd.service → /lib/systemd/system/containerd.service.
Setting up patch (2.7.6-7) ...
root@vps61268:~# ^[[200~docker compose -f /opt/mssql/docker-compose.yml up -d
-bash: $'\E[200~docker': command not found
root@vps61268:~# docker compose -f /opt/mssql/docker-compose.yml up -d
[+] Running 4/4
 ✔ mssql Pulled                                                                                                  104.9s
   ✔ 87ea81311979 Pull complete                                                                                    5.4s
   ✔ 596d80c4d18d Pull complete                                                                                  101.2s
   ✔ 49f7ff1966d5 Pull complete                                                                                  104.0s
[+] Running 3/3
 ✔ Network mssql_default      Created                                                                              0.1s
 ✔ Volume "mssql_mssql-data"  Created                                                                              0.0s
 ✔ Container mssql            Started                                                                              0.5s
root@vps61268:~# docker ps
CONTAINER ID   IMAGE                                        COMMAND                  CREATED          STATUS                         PORTS     NAMES
e222e122c0a4   mcr.microsoft.com/mssql/server:2022-latest   "/opt/mssql/bin/laun…"   34 seconds ago   Restarting (1) 5 seconds ago             mssql
root@vps61268:~# docker logs mssql --tail=50
/opt/mssql/bin/sqlservr: This program requires a machine with at least 2000 megabytes of memory.

SQL Server 2022 will run as non-root by default.
This container is running as user mssql.
To learn more visit https://go.microsoft.com/fwlink/?linkid=2099216.
sqlservr: This program requires a machine with at least 2000 megabytes of memory.
/opt/mssql/bin/sqlservr: This program requires a machine with at least 2000 megabytes of memory.

SQL Server 2022 will run as non-root by default.
This container is running as user mssql.
To learn more visit https://go.microsoft.com/fwlink/?linkid=2099216.
sqlservr: This program requires a machine with at least 2000 megabytes of memory.
/opt/mssql/bin/sqlservr: This program requires a machine with at least 2000 megabytes of memory.

SQL Server 2022 will run as non-root by default.
This container is running as user mssql.
To learn more visit https://go.microsoft.com/fwlink/?linkid=2099216.
sqlservr: This program requires a machine with at least 2000 megabytes of memory.
/opt/mssql/bin/sqlservr: This program requires a machine with at least 2000 megabytes of memory.

SQL Server 2022 will run as non-root by default.
This container is running as user mssql.
To learn more visit https://go.microsoft.com/fwlink/?linkid=2099216.
sqlservr: This program requires a machine with at least 2000 megabytes of memory.
/opt/mssql/bin/sqlservr: This program requires a machine with at least 2000 megabytes of memory.

SQL Server 2022 will run as non-root by default.
This container is running as user mssql.
To learn more visit https://go.microsoft.com/fwlink/?linkid=2099216.
sqlservr: This program requires a machine with at least 2000 megabytes of memory.
/opt/mssql/bin/sqlservr: This program requires a machine with at least 2000 megabytes of memory.

SQL Server 2022 will run as non-root by default.
This container is running as user mssql.
To learn more visit https://go.microsoft.com/fwlink/?linkid=2099216.
sqlservr: This program requires a machine with at least 2000 megabytes of memory.
/opt/mssql/bin/sqlservr: This program requires a machine with at least 2000 megabytes of memory.

SQL Server 2022 will run as non-root by default.
This container is running as user mssql.
To learn more visit https://go.microsoft.com/fwlink/?linkid=2099216.
sqlservr: This program requires a machine with at least 2000 megabytes of memory.
/opt/mssql/bin/sqlservr: This program requires a machine with at least 2000 megabytes of memory.

SQL Server 2022 will run as non-root by default.
This container is running as user mssql.
To learn more visit https://go.microsoft.com/fwlink/?linkid=2099216.
sqlservr: This program requires a machine with at least 2000 megabytes of memory.
/opt/mssql/bin/sqlservr: This program requires a machine with at least 2000 megabytes of memory.

root@vps61268:~# docker exec -it mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'ChangeThisStrong!Password123'
Error response from daemon: Container e222e122c0a4e64acee17331c663190eb3837d9cd4164cbedee1556c5d09d3a3 is restarting, wait until the container is running
root@vps61268:~# docker compose -f /opt/mssql/docker-compose.yml down
[+] Running 2/2
 ✔ Container mssql        Removed                                                                                  0.0s
 ✔ Network mssql_default  Removed                                                                                  0.1s
root@vps61268:~# docker compose -f /opt/mssql/docker-compose.yml up -d
[+] Running 2/2
 ✔ Network mssql_default  Created                                                                                  0.1s
 ✔ Container mssql        Started                                                                                  0.4s
root@vps61268:~# docker ps
CONTAINER ID   IMAGE                                        COMMAND                  CREATED          STATUS                          PORTS     NAMES
98b57fcf67a2   mcr.microsoft.com/mssql/server:2022-latest   "/opt/mssql/bin/laun…"   52 seconds ago   Restarting (1) 23 seconds ago             mssql
root@vps61268:~# docker logs mssql --tail=50
/opt/mssql/bin/sqlservr: This program requires a machine with at least 2000 megabytes of memory.

SQL Server 2022 will run as non-root by default.
This container is running as user mssql.
To learn more visit https://go.microsoft.com/fwlink/?linkid=2099216.
sqlservr: This program requires a machine with at least 2000 megabytes of memory.
/opt/mssql/bin/sqlservr: This program requires a machine with at least 2000 megabytes of memory.

SQL Server 2022 will run as non-root by default.
This container is running as user mssql.
To learn more visit https://go.microsoft.com/fwlink/?linkid=2099216.
sqlservr: This program requires a machine with at least 2000 megabytes of memory.
/opt/mssql/bin/sqlservr: This program requires a machine with at least 2000 megabytes of memory.

SQL Server 2022 will run as non-root by default.
This container is running as user mssql.
To learn more visit https://go.microsoft.com/fwlink/?linkid=2099216.
sqlservr: This program requires a machine with at least 2000 megabytes of memory.
/opt/mssql/bin/sqlservr: This program requires a machine with at least 2000 megabytes of memory.

SQL Server 2022 will run as non-root by default.
This container is running as user mssql.
To learn more visit https://go.microsoft.com/fwlink/?linkid=2099216.
sqlservr: This program requires a machine with at least 2000 megabytes of memory.
/opt/mssql/bin/sqlservr: This program requires a machine with at least 2000 megabytes of memory.

SQL Server 2022 will run as non-root by default.
This container is running as user mssql.
To learn more visit https://go.microsoft.com/fwlink/?linkid=2099216.
sqlservr: This program requires a machine with at least 2000 megabytes of memory.
/opt/mssql/bin/sqlservr: This program requires a machine with at least 2000 megabytes of memory.

SQL Server 2022 will run as non-root by default.
This container is running as user mssql.
To learn more visit https://go.microsoft.com/fwlink/?linkid=2099216.
sqlservr: This program requires a machine with at least 2000 megabytes of memory.
/opt/mssql/bin/sqlservr: This program requires a machine with at least 2000 megabytes of memory.

SQL Server 2022 will run as non-root by default.
This container is running as user mssql.
To learn more visit https://go.microsoft.com/fwlink/?linkid=2099216.
sqlservr: This program requires a machine with at least 2000 megabytes of memory.
/opt/mssql/bin/sqlservr: This program requires a machine with at least 2000 megabytes of memory.

SQL Server 2022 will run as non-root by default.
This container is running as user mssql.
To learn more visit https://go.microsoft.com/fwlink/?linkid=2099216.
sqlservr: This program requires a machine with at least 2000 megabytes of memory.
/opt/mssql/bin/sqlservr: This program requires a machine with at least 2000 megabytes of memory.

root@vps61268:~# docker exec -it mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'ChangeThisStrong!Password123' -Q "SELECT @@VERSION;"
Error response from daemon: Container 98b57fcf67a2c8888a9f498928e2996b87424633787693d2e7ecd6ac332fe11f is restarting, wait until the container is running
root@vps61268:~# ufw delete allow 1433/tcp
Rule deleted
Rule deleted (v6)
root@vps61268:~# apt update
Hit:1 http://security.debian.org/debian-security bookworm-security InRelease
Hit:2 http://debian.mirror.locaweb.com.br bookworm InRelease
Hit:3 http://debian.mirror.locaweb.com.br bookworm-updates InRelease
Hit:4 https://packages.microsoft.com/debian/12/prod bookworm InRelease
Hit:5 https://download.docker.com/linux/debian bookworm InRelease
Reading package lists... Done
Building dependency tree... Done
Reading state information... Done
All packages are up to date.
root@vps61268:~# apt install -y postgresql postgresql-contrib
Reading package lists... Done
Building dependency tree... Done
Reading state information... Done
The following additional packages will be installed:
  libcommon-sense-perl libjson-perl libjson-xs-perl libllvm14 libpq5 libsensors-config libsensors5
  libtypes-serialiser-perl libxslt1.1 libz3-4 postgresql-15 postgresql-client-15 postgresql-client-common
  postgresql-common ssl-cert sysstat
Suggested packages:
  lm-sensors postgresql-doc postgresql-doc-15 isag
The following NEW packages will be installed:
  libcommon-sense-perl libjson-perl libjson-xs-perl libllvm14 libpq5 libsensors-config libsensors5
  libtypes-serialiser-perl libxslt1.1 libz3-4 postgresql postgresql-15 postgresql-client-15 postgresql-client-common
  postgresql-common postgresql-contrib ssl-cert sysstat
0 upgraded, 18 newly installed, 0 to remove and 0 not upgraded.
Need to get 49.2 MB of archives.
After this operation, 201 MB of additional disk space will be used.
Get:1 http://debian.mirror.locaweb.com.br bookworm/main amd64 libjson-perl all 4.10000-1 [87.5 kB]
Get:2 http://debian.mirror.locaweb.com.br bookworm/main amd64 postgresql-client-common all 248+deb12u1 [35.2 kB]
Get:3 http://debian.mirror.locaweb.com.br bookworm/main amd64 ssl-cert all 1.1.2 [21.1 kB]
Get:4 http://debian.mirror.locaweb.com.br bookworm/main amd64 postgresql-common all 248+deb12u1 [179 kB]
Get:5 http://debian.mirror.locaweb.com.br bookworm/main amd64 libcommon-sense-perl amd64 3.75-3 [23.0 kB]
Get:6 http://debian.mirror.locaweb.com.br bookworm/main amd64 libtypes-serialiser-perl all 1.01-1 [12.2 kB]
Get:7 http://debian.mirror.locaweb.com.br bookworm/main amd64 libjson-xs-perl amd64 4.030-2+b1 [92.1 kB]
Get:8 http://debian.mirror.locaweb.com.br bookworm/main amd64 libz3-4 amd64 4.8.12-3.1 [7,216 kB]
Get:9 http://debian.mirror.locaweb.com.br bookworm/main amd64 libllvm14 amd64 1:14.0.6-12 [21.8 MB]
Get:10 http://debian.mirror.locaweb.com.br bookworm/main amd64 libpq5 amd64 15.14-0+deb12u1 [194 kB]
Get:11 http://debian.mirror.locaweb.com.br bookworm/main amd64 libsensors-config all 1:3.6.0-7.1 [14.3 kB]
Get:12 http://debian.mirror.locaweb.com.br bookworm/main amd64 libsensors5 amd64 1:3.6.0-7.1 [34.2 kB]
Get:13 http://debian.mirror.locaweb.com.br bookworm/main amd64 libxslt1.1 amd64 1.1.35-1+deb12u2 [231 kB]
Get:14 http://debian.mirror.locaweb.com.br bookworm/main amd64 postgresql-client-15 amd64 15.14-0+deb12u1 [1,731 kB]
Get:15 http://debian.mirror.locaweb.com.br bookworm/main amd64 postgresql-15 amd64 15.14-0+deb12u1 [16.9 MB]
Get:16 http://debian.mirror.locaweb.com.br bookworm/main amd64 postgresql all 15+248+deb12u1 [10.2 kB]
Get:17 http://debian.mirror.locaweb.com.br bookworm/main amd64 postgresql-contrib all 15+248+deb12u1 [10.2 kB]
Get:18 http://debian.mirror.locaweb.com.br bookworm/main amd64 sysstat amd64 12.6.1-1 [596 kB]
Fetched 49.2 MB in 2s (29.5 MB/s)
Preconfiguring packages ...
Selecting previously unselected package libjson-perl.
(Reading database ... 43454 files and directories currently installed.)
Preparing to unpack .../00-libjson-perl_4.10000-1_all.deb ...
Unpacking libjson-perl (4.10000-1) ...
Selecting previously unselected package postgresql-client-common.
Preparing to unpack .../01-postgresql-client-common_248+deb12u1_all.deb ...
Unpacking postgresql-client-common (248+deb12u1) ...
Selecting previously unselected package ssl-cert.
Preparing to unpack .../02-ssl-cert_1.1.2_all.deb ...
Unpacking ssl-cert (1.1.2) ...
Selecting previously unselected package postgresql-common.
Preparing to unpack .../03-postgresql-common_248+deb12u1_all.deb ...
Adding 'diversion of /usr/bin/pg_config to /usr/bin/pg_config.libpq-dev by postgresql-common'
Unpacking postgresql-common (248+deb12u1) ...
Selecting previously unselected package libcommon-sense-perl:amd64.
Preparing to unpack .../04-libcommon-sense-perl_3.75-3_amd64.deb ...
Unpacking libcommon-sense-perl:amd64 (3.75-3) ...
Selecting previously unselected package libtypes-serialiser-perl.
Preparing to unpack .../05-libtypes-serialiser-perl_1.01-1_all.deb ...
Unpacking libtypes-serialiser-perl (1.01-1) ...
Selecting previously unselected package libjson-xs-perl.
Preparing to unpack .../06-libjson-xs-perl_4.030-2+b1_amd64.deb ...
Unpacking libjson-xs-perl (4.030-2+b1) ...
Selecting previously unselected package libz3-4:amd64.
Preparing to unpack .../07-libz3-4_4.8.12-3.1_amd64.deb ...
Unpacking libz3-4:amd64 (4.8.12-3.1) ...
Selecting previously unselected package libllvm14:amd64.
Preparing to unpack .../08-libllvm14_1%3a14.0.6-12_amd64.deb ...
Unpacking libllvm14:amd64 (1:14.0.6-12) ...
Selecting previously unselected package libpq5:amd64.
Preparing to unpack .../09-libpq5_15.14-0+deb12u1_amd64.deb ...
Unpacking libpq5:amd64 (15.14-0+deb12u1) ...
Selecting previously unselected package libsensors-config.
Preparing to unpack .../10-libsensors-config_1%3a3.6.0-7.1_all.deb ...
Unpacking libsensors-config (1:3.6.0-7.1) ...
Selecting previously unselected package libsensors5:amd64.
Preparing to unpack .../11-libsensors5_1%3a3.6.0-7.1_amd64.deb ...
Unpacking libsensors5:amd64 (1:3.6.0-7.1) ...
Selecting previously unselected package libxslt1.1:amd64.
Preparing to unpack .../12-libxslt1.1_1.1.35-1+deb12u2_amd64.deb ...
Unpacking libxslt1.1:amd64 (1.1.35-1+deb12u2) ...
Selecting previously unselected package postgresql-client-15.
Preparing to unpack .../13-postgresql-client-15_15.14-0+deb12u1_amd64.deb ...
Unpacking postgresql-client-15 (15.14-0+deb12u1) ...
Selecting previously unselected package postgresql-15.
Preparing to unpack .../14-postgresql-15_15.14-0+deb12u1_amd64.deb ...
Unpacking postgresql-15 (15.14-0+deb12u1) ...
Selecting previously unselected package postgresql.
Preparing to unpack .../15-postgresql_15+248+deb12u1_all.deb ...
Unpacking postgresql (15+248+deb12u1) ...
Selecting previously unselected package postgresql-contrib.
Preparing to unpack .../16-postgresql-contrib_15+248+deb12u1_all.deb ...
Unpacking postgresql-contrib (15+248+deb12u1) ...
Selecting previously unselected package sysstat.
Preparing to unpack .../17-sysstat_12.6.1-1_amd64.deb ...
Unpacking sysstat (12.6.1-1) ...
Setting up postgresql-client-common (248+deb12u1) ...
Setting up libsensors-config (1:3.6.0-7.1) ...
Setting up libpq5:amd64 (15.14-0+deb12u1) ...
Setting up libcommon-sense-perl:amd64 (3.75-3) ...
Setting up postgresql-client-15 (15.14-0+deb12u1) ...
update-alternatives: using /usr/share/postgresql/15/man/man1/psql.1.gz to provide /usr/share/man/man1/psql.1.gz (psql.1.gz) in auto mode
Setting up libz3-4:amd64 (4.8.12-3.1) ...
Setting up ssl-cert (1.1.2) ...
Setting up libsensors5:amd64 (1:3.6.0-7.1) ...
Setting up libllvm14:amd64 (1:14.0.6-12) ...
Setting up libtypes-serialiser-perl (1.01-1) ...
Setting up libjson-perl (4.10000-1) ...
Setting up libxslt1.1:amd64 (1.1.35-1+deb12u2) ...
Setting up sysstat (12.6.1-1) ...

Creating config file /etc/default/sysstat with new version
update-alternatives: using /usr/bin/sar.sysstat to provide /usr/bin/sar (sar) in auto mode
Created symlink /etc/systemd/system/sysstat.service.wants/sysstat-collect.timer → /lib/systemd/system/sysstat-collect.timer.
Created symlink /etc/systemd/system/sysstat.service.wants/sysstat-summary.timer → /lib/systemd/system/sysstat-summary.timer.
Created symlink /etc/systemd/system/multi-user.target.wants/sysstat.service → /lib/systemd/system/sysstat.service.
Setting up libjson-xs-perl (4.030-2+b1) ...
Setting up postgresql-common (248+deb12u1) ...

Creating config file /etc/postgresql-common/createcluster.conf with new version
Building PostgreSQL dictionaries from installed myspell/hunspell packages...
Removing obsolete dictionary files:
Created symlink /etc/systemd/system/multi-user.target.wants/postgresql.service → /lib/systemd/system/postgresql.service.
Setting up postgresql-15 (15.14-0+deb12u1) ...
Creating new PostgreSQL cluster 15/main ...
/usr/lib/postgresql/15/bin/initdb -D /var/lib/postgresql/15/main --auth-local peer --auth-host scram-sha-256 --no-instructions
The files belonging to this database system will be owned by user "postgres".
This user must also own the server process.

The database cluster will be initialized with locale "en_US.UTF-8".
The default database encoding has accordingly been set to "UTF8".
The default text search configuration will be set to "english".

Data page checksums are disabled.

fixing permissions on existing directory /var/lib/postgresql/15/main ... ok
creating subdirectories ... ok
selecting dynamic shared memory implementation ... posix
selecting default max_connections ... 100
selecting default shared_buffers ... 128MB
selecting default time zone ... America/Sao_Paulo
creating configuration files ... ok
running bootstrap script ... ok
performing post-bootstrap initialization ... ok
syncing data to disk ... ok
update-alternatives: using /usr/share/postgresql/15/man/man1/postmaster.1.gz to provide /usr/share/man/man1/postmaster.1.gz (postmaster.1.gz) in auto mode
Setting up postgresql-contrib (15+248+deb12u1) ...
Setting up postgresql (15+248+deb12u1) ...
Processing triggers for libc-bin (2.36-9+deb12u13) ...
root@vps61268:~# sudo -u postgres psql -c "ALTER USER postgres WITH PASSWORD 'StrongPg!123';"
-bash: !123: event not found
root@vps61268:~# -u postgres psql -c "ALTER USER postgres WITH PASSWORD 'StrongPg!123';"
-bash: !123: event not found
root@vps61268:~# -u postgres psql -c "ALTER USER postgres WITH PASSWORD 'StrongPg!123';"
-bash: !123: event not found
root@vps61268:~# sudo -u postgres psql -c "ALTER USER postgres WITH PASSWORD 'StrongPg!123';"
-bash: !123: event not found
root@vps61268:~# sudo -u postgres createdb DbLevelUp
could not change directory to "/root": Permission denied
root@vps61268:~# su - postgres
postgres@vps61268:~$ psql -c "ALTER USER postgres WITH PASSWORD 'StrongPg!123';"
-bash: !123: event not found
postgres@vps61268:~$ psql -c "ALTER USER postgres WITH PASSWORD 'StrongPg\!123';"
ALTER ROLE
postgres@vps61268:~$ psql
psql (15.14 (Debian 15.14-0+deb12u1))
Type "help" for help.

postgres=# CREATE DATABASE dblevelup;
CREATE DATABASE
postgres=# CREATE USER dbleveluser WITH PASSWORD 'Dbl3vel!Up123';
CREATE ROLE
postgres=# GRANT ALL PRIVILEGES ON DATABASE dblevelup TO dbleveluser;
GRANT
postgres=# \l
                                                   List of databases
   Name    |  Owner   | Encoding |   Collate   |    Ctype    | ICU Locale | Locale Provider |    Access privileges
-----------+----------+----------+-------------+-------------+------------+-----------------+--------------------------
 DbLevelUp | postgres | UTF8     | en_US.UTF-8 | en_US.UTF-8 |            | libc            |
 dblevelup | postgres | UTF8     | en_US.UTF-8 | en_US.UTF-8 |            | libc            | =Tc/postgres            +
           |          |          |             |             |            |                 | postgres=CTc/postgres   +
           |          |          |             |             |            |                 | dbleveluser=CTc/postgres
 postgres  | postgres | UTF8     | en_US.UTF-8 | en_US.UTF-8 |            | libc            |
 template0 | postgres | UTF8     | en_US.UTF-8 | en_US.UTF-8 |            | libc            | =c/postgres             +
           |          |          |             |             |            |                 | postgres=CTc/postgres
 template1 | postgres | UTF8     | en_US.UTF-8 | en_US.UTF-8 |            | libc            | =c/postgres             +
           |          |          |             |             |            |                 | postgres=CTc/postgres
(5 rows)

postgres=# \q
postgres@vps61268:~$ ufw allow 5432/tcp
-bash: ufw: command not found
postgres@vps61268:~$ \q
-bash: q: command not found
postgres@vps61268:~$ exit
logout
root@vps61268:~# ufw allow 5432/tcp
Rule added
Rule added (v6)
root@vps61268:~# ufw reload
Firewall reloaded
root@vps61268:~# ufw status verbose
Status: active
Logging: on (low)
Default: deny (incoming), allow (outgoing), deny (routed)
New profiles: skip

To                         Action      From
--                         ------      ----
22/tcp (OpenSSH)           ALLOW IN    Anywhere
80/tcp                     ALLOW IN    Anywhere
443/tcp                    ALLOW IN    Anywhere
5432/tcp                   ALLOW IN    Anywhere
22/tcp (OpenSSH (v6))      ALLOW IN    Anywhere (v6)
80/tcp (v6)                ALLOW IN    Anywhere (v6)
443/tcp (v6)               ALLOW IN    Anywhere (v6)
5432/tcp (v6)              ALLOW IN    Anywhere (v6)

root@vps61268:~# nano /etc/postgresql/15/main/postgresql.conf
root@vps61268:~# nano /etc/postgresql/15/main/pg_hba.conf
root@vps61268:~# systemctl restart postgresql
root@vps61268:~#