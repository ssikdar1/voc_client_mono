# voc_client mono<br>
This is a VOC reference client application for Mono an open source implementation of Microsoft's .NET/C# Framework.<br>


Install mono-complete:
https://askubuntu.com/questions/497358/how-to-install-mono-on-ubuntu-64-bit-v14-04
sudo apt-get install mono-complete <br/>


For JSON lib:
http://stackoverflow.com/questions/24049992/json-net-on-ubuntu-linux
<br/>

How I compile right now:
ssikdar@bos-lpbwl:~/voc_client_mono$ mcs voc_client.cs -r:/usr/lib/cli/Newtonsoft.Json-5.0/Newtonsoft.Json.dll -r:/usr/lib/mono/4.5/Mono.Data.Sqlite.dll -r:/usr/lib/mono/4.5/System.Data.dll 

Run:
sikdar@bos-lpbwl:~/voc_client_mono$ mono voc_client.exe 

To compile to a dll:
ssikdar@bos-lpbwl:~/voc_client_mono$ mcs /target:library voc_client.cs  -r:/usr/lib/cli/Newtonsoft.Json-5.0/Newtonsoft.Json.dll -r:/usr/lib/mono/4.5/Mono.Data.Sqlite.dll -r:/usr/lib/mono/4.5/System.Data.dll 


<br/>
