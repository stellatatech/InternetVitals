# Internet Vitals
Internet Vitals is a console application built using C# to monitor networking speed and information about the local network.

NOTE: This project requires .NET 6 to properly run.

In the future, a binary may be released, but for now, you can build from source by opening in a multitude of IDEs or whatever your preferred method for creating executables. You can then run the binary (example below), and experience the program first-hand:

View all available commands by running this:

```bash
./pathToBinary/bin/InternetVitals get-vitals -h
```

More commands are available, but you can run this to view all network metrics in a single command:

### macOS/Linux
```bash
./pathToBinary/bin/InternetVitals get-vitals -a
```

### Windows
```powershell
./pathToBinary/bin/InternetVitals get-vitals -a
```