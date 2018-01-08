#bin/sh
sudo mcs -r:/usr/lib/mono/gac/nunit.framework/2.6.0.0__96d09a1eb7f44a77/nunit.framework.dll main_test.cs protocol.cs protocol_test.cs /out:NUnit.exe && nunit-console NUnit.exe
