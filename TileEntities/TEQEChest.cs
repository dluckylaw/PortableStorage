﻿using PortableStorage.Tiles;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TheOneLibrary.Base;
using TheOneLibrary.Storage;
using TheOneLibrary.Utility;

namespace PortableStorage.TileEntities
{
	public class TEQEChest : BaseTE, IContainerTile
	{
		public override bool ValidTile(Tile tile) => tile.type == mod.TileType<QEChest>() && tile.TopLeft();

		public Frequency frequency = new Frequency(Colors.White);
		public int animState;
		public int animTimer;

		public bool opened = false;

		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient) return Place(i, j - 1);

			NetMessage.SendTileSquare(Main.myPlayer, i, j - 1, 2);
			NetMessage.SendData(MessageID.TileEntityPlacement, number: i, number2: j - 1, number3: Type);

			return -1;
		}

		public override void Update()
		{
			if (opened && animState < 2)
			{
				if (++animTimer >= 10)
				{
					animState++;
					animTimer = 0;
				}
			}
			else if (!opened && animState > 0)
			{
				if (++animTimer >= 10)
				{
					animState--;
					animTimer = 0;
				}
			}

			Main.tile[Position.X, Position.Y].frameX = (short)(animState * 36);
			Main.tile[Position.X, Position.Y + 1].frameX = (short)(animState * 36);
			Main.tile[Position.X + 1, Position.Y].frameX = (short)(animState * 36 + 18);
			Main.tile[Position.X + 1, Position.Y + 1].frameX = (short)(animState * 36 + 18);

			this.HandleUIFar();
		}

		public override TagCompound Save() => new TagCompound
		{
			["Frequency"] = frequency
		};

		public override void Load(TagCompound tag)
		{
			frequency = tag.Get<Frequency>("Frequency");
		}

		public override void NetSend(BinaryWriter writer, bool lightSend)
		{
			writer.Write((int)frequency.colorLeft);
			writer.Write((int)frequency.colorMiddle);
			writer.Write((int)frequency.colorRight);
		}

		public override void NetReceive(BinaryReader reader, bool lightReceive)
		{
			frequency.colorLeft = (Colors)reader.ReadInt32();
			frequency.colorMiddle = (Colors)reader.ReadInt32();
			frequency.colorRight = (Colors)reader.ReadInt32();
		}

		public IList<Item> GetItems() => mod.GetModWorld<PSWorld>().enderItems[frequency];

		public ModTileEntity GetTileEntity() => this;
	}
}