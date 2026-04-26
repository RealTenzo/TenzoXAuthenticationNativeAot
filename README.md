# TXC Authentication Native AOT

Small WinForms example project for the TXC authentication SDK with Native AOT enabled.

This sample includes:

- a simple login/register desktop UI
- SDK initialization on startup
- signed response validation through the embedded public key
- Native AOT-friendly JSON source generation in the SDK layer

## Project layout

- `TXC_Authentication_NativeAot.sln` - solution file
- `TXC_Authentication_NativeAot/Form1.cs` - example UI logic
- `TXC_Authentication_NativeAot/Form1.Designer.cs` - form layout
- `TXC_Authentication_NativeAot/txa_native.cs` - SDK implementation

## Setup

Open `TXC_Authentication_NativeAot/Form1.cs` and update the app credentials:

```csharp
private static readonly TXA Auth = new TXA(
    name: "test",
    secret: "TXA-ZKOM4LP9DA",
    version: "1.0"
);
```

Replace those values with your own application name, secret, and version.

## Run

From the solution folder:

```powershell
dotnet build
dotnet run --project .\TXC_Authentication_NativeAot\TXC_Authentication_NativeAot.csproj
```

Or open the solution in Visual Studio and run it normally.

## Publish Native AOT

```powershell
dotnet publish .\TXC_Authentication_NativeAot\TXC_Authentication_NativeAot.csproj -c Release
```

## Notes

- The form initializes the SDK when the window is shown.
- Login and register buttons are already wired to the SDK.
- If you change the backend signing key, update the embedded public key in `txa_native.cs` and rebuild.
