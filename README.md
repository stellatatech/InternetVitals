# NetVitals
NetVitals is a command line application built to monitor networking speed and collect information about the local network configuration.

## Usage

View all available arguments by running the following command:

```bash
NetVitals -h
```

## Commands

Below you'll find a table of all available commands for the NetVitals application:

<table style="table-layout:fixed; white-space: nowrap;">
  <tr>
    <th>Command</th>
    <th>Example</th>
    <th>Description</th>
  </tr>
  <tr>
    <td><a href="#monitor">monitor</a></td>
    <td>NetVitals monitor -ARG</td>
    <td>NetVitals is a command line application built to monitor networking speed and collect information about the local network configuration</td>
  </tr>
</table>

### Monitor

<table style="table-layout:fixed; white-space: nowrap;">
  <tr>
    <th>Command (short)</th>
    <th>Command (long)</th>
    <th>Description</th>
  </tr>
  <tr>
    <td>NetVitals monitor -a</td>
    <td>NetVitals monitor --all</td>
    <td>Collect all available metrics</td>
  </tr>
  <tr>
    <td>NetVitals monitor -c</td>
    <td>NetVitals monitor --connect</td>
    <td>Validates connection request</td>
  </tr>
  <tr>
    <td>NetVitals monitor -i</td>
    <td>NetVitals monitor --internal-ip</td>
    <td>Collect internal IP Address</td>
  </tr>
  <tr>
    <td>NetVitals monitor -e</td>
    <td>NetVitals monitor --external-ip</td>
    <td>Collect external IP Address</td>
  </tr>
  <tr>
    <td>NetVitals monitor -p</td>
    <td>NetVitals monitor --ping</td>
    <td>Get ping speed</td>
  </tr>
  <tr>
    <td>NetVitals monitor -s</td>
    <td>NetVitals monitor --net-stats</td>
    <td>Get internet stats for all network adapters on the device</td>
  </tr>
  <tr>
    <td>NetVitals monitor -d</td>
    <td>NetVitals monitor --download</td>
    <td>Test download speed</td>
  </tr>
  <tr>
    <td>NetVitals monitor -v</td>
    <td>NetVitals monitor --version</td>
    <td>NetVitals version information</td>
  </tr>
</table>

## Build

An executable is currently available for download under [Releases](https://github.com/stellatatech/NetVitals/releases), but if you're interested in building from source, you can easily compile the project by running the following command:

```bash
dotnet build
```