# miditoTcp
Transmits incoming MIDI data to TCP

Supports multi-port

## How to use
Edit this line, (midi device name and port)
https://github.com/dhlrunner/miditoTcp/blob/2da38ad1111024d75c1024b5129bfd42ec323155/miditoTcp/Program.cs#L28

and this line (IP and port)
https://github.com/dhlrunner/miditoTcp/blob/2da38ad1111024d75c1024b5129bfd42ec323155/miditoTcp/Program.cs#L20

and build, start it

## Build
Needs Visual Studio 2022, tested at windows 10 and 11

## Data structure
|Byte No|Value|Explane|
|-|-|-|
|0|0xFF|STX|
|1|0xXX|Follow-up data length|
|2|0xF5|Port Specified Start Byte (Is this MIDI standard?)|
|3|0xXX|Port number|
|4|0xXX|Midi message data|
|...||MIDI data continues|

## Example data transmitted by this program
Note-On to MIDI port 1

```0xFF 0x05 0xF5 0x01 0x90 0x45 0x7F```

Send GS Reset SysEx to MIDI port 2

```0xFF 0x0D 0xF5 0x02 0xF0 0x41 0x10 0x42 0x12 0x40 0x00 0x7F 0x00 0x41 0xF7```
