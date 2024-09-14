using Godot;
using QRCoder;
using System;
using System.Linq;
using System.Net.Sockets;

public static class SPJ
{
	public static SPJMeta Meta;
	public static SPJGameServer GameServer;
	public static SPJHttpServer HttpServer;
	private static QRCodeGenerator generator = new QRCodeGenerator();

	public static CSharpScript LoadModuleClass(string name)
	{
		return GD.Load<CSharpScript>($"res://games/{name}/{name.Capitalize()}Module.cs");
	}

	public static SPJModuleMetadata[] GetMinigames()
	{
		var games = DirAccess.GetDirectoriesAt("res://games/");
		var buttonBase = GD.Load<PackedScene>("res://global/lobby/modulebutton.tscn").Instantiate<ModuleButton>();
		SPJModuleMetadata[] modules = new SPJModuleMetadata[games.Length];
		var index = 0;
		foreach (var game in games)
		{
			var button = buttonBase.Duplicate() as ModuleButton;
			modules.SetValue(LoadModuleClass(game).New().As<SPJModule>().GetMetadata(), index++);
		}
		return modules;
	}

	public static int RunServer(TcpServer server, int basePort)
	{
		var port = (ushort)basePort;
		while (true)
		{
			var error = server.Listen(port);
			if (error == Error.Ok)
			{
				return port;
			}
			port += 1;
		}
	}

	public static string GetSelfIp()
	{
		UdpClient udpClient = new UdpClient(60000);
		// this may seem insane but we don't actually need to connect anywhere,
		// just get the local endpoint
		var addr = new System.Net.IPEndPoint(1, 1);
		udpClient.Connect(addr);
		try
		{
			return udpClient.Client.LocalEndPoint.ToString().Split(":")[0];
		}
		catch
		{
			return "";
		}
	}

	public static Image GetQRCode(string text)
	{
		var white = Color.Color8(255, 255, 255);
		var black = Color.Color8(0, 0, 0);
		var qr = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
		var data = qr.GetRawData(QRCodeData.Compression.Uncompressed);
		var sideLength = data[4];
		var croppedLength = sideLength - 6;
		var image = Image.CreateEmpty(croppedLength, croppedLength, false, Image.Format.L8);
		var i = 0;
		var max = sideLength * sideLength;
		image.Fill(white);
		foreach (var currentByte in data.Skip(5))
		{
			for (var bitIndex = 0; bitIndex < 8; bitIndex++)
			{
				var bit = (currentByte & (1 << (7 - bitIndex))) != 0;
				if (bit)
				{
					var x = i % sideLength;
					var y = i / sideLength;
					x -= 3;
					y -= 3;
					if (x < 0 || y < 0 || x > croppedLength || y > croppedLength)
					{
						continue;
					}
					image.SetPixel(x, y, black);
				}
				i++;
				if (i >= max)
				{
					return image;
				}
			}
		}
		return image;
	}
}