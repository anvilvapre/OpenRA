#region Copyright & License Information
/*
 * Copyright 2007-2016 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Scripting;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets.Logic
{
	public class MapGeneratorMainMenuLogic: ChromeLogic
	{
		Widget menu;

                private MPos WriteTerrainTemplate(MPos mapPos, Map map, TileSet tileSet, ushort tileId) {
                        TerrainTemplateInfo tti = tileSet.Templates[tileId];
                        int2 size = tti.Size;
                        Log.Write("vapre", 
                                "WriteTerrainTemplate, posU {0}, posV {1}, tileW {2}, tileH {3} templateId {4}, mapW {5} mapH {6} \n",
                                        mapPos.U,
                                        mapPos.V,
                                        size.X,
                                        size.Y,
                                        tileId,
                                        map.MapSize.X,
                                        map.MapSize.Y);
                        if (mapPos.U + size.X >= map.MapSize.X || mapPos.V + size.Y >= map.MapSize.Y) {
                                Log.Write("vapre",  "============== WriteTerrainTemplate out of bounds");
                                return mapPos;
                        }
                        byte index = 0;
                        for (int i=0;i<size.X;i++) {
                                for (int j=0;j<size.Y;j++) {
                                        // tile uint16
                                        map.Tiles[new MPos(mapPos.U + i, mapPos.V + j)] = new TerrainTile(tileId, index++);
                                }
                        }
                        int u = mapPos.U + size.X + 2;
                        int v = mapPos.V;
                        if (u > map.MapSize.X - 2) {
                                u = 0;
                                v += 8;
                        }
                        return new MPos(u, v);
                }

		[ObjectCreator.UseCtor]
		public MapGeneratorMainMenuLogic(Widget widget, ModData modData, World world, Action onExit)
		{
                        bool visible = true;
                        widget.IsVisible = () => visible;
                        Action actionShowTileTraitsPanel = () => { 
                                widget.IsVisible = () => false;
                                Game.OpenWindow("MAP_GENERATOR_TILE_TRAITS_PANEL", new WidgetArgs()
				{
					{ "onExit", () => widget.IsVisible = () => visible },
				});
                        };
                        Action actionListTileSet = () => {
                                var map = world.Map;
                                var tileSet = modData.DefaultTileSets[map.Tileset];
                                Log.AddChannel("vapre", "vapre.log");
                                Log.Write("vapre", "Tileset name:{0} id:{1} pal:{2} playerpal:{3} sheetsize:{4}\n", 
                                                tileSet.Name, 
                                                tileSet.Id, 
                                                tileSet.Palette, 
                                                tileSet.PlayerPalette, 
                                                tileSet.SheetSize);
                                foreach (var ti in tileSet.TerrainInfo) {
                                        //var ti = tileSet.TerrainInfo;
                                        Log.Write("vapre", "Tileset terrain (type) info, name:{0} iswater:{1} color:{2} custom_cursor:{3}\n",
                                                ti.Type, 
                                                ti.IsWater,
                                                ti.Color,
                                                ti.CustomCursor);

                                        foreach (var tt in ti.TargetTypes) {
                                                Log.Write("vapre", "Tileset terrain (type) info, target type: {0}\n",
                                                        tt);
                                        }
                                        foreach (var st in ti.AcceptsSmudgeType) {
                                                Log.Write("vapre", "Tileset terrain (type) info, acceptssmudgetype type: {0}\n",
                                                        st);
                                        }

                                }
                                MPos mapPos = new MPos(0, 0);
                                foreach (var pair in tileSet.Templates) {
                                        Log.Write("vapre", 
                                                "TerrainTemplateInfo, key:{0}, id:{1}, ImagesCount:{2}, Category:{3}, Palette:{4}, SizeX:{5}, SizeY:{6}\n",
                                                pair.Key,
                                                pair.Value.Id,
                                                pair.Value.Images.Length,
                                                pair.Value.Category,
                                                pair.Value.Palette,
                                                pair.Value.Size.X,
                                                pair.Value.Size.Y
                                                );
                                        foreach (var i in pair.Value.Images) {
                                                Log.Write("vapre", "TerrainTemplateInfo, images: {0}\n",
                                                                i);
                                        }

                                        var c = pair.Value.TilesCount;
                                        for (int i=0;i<c;i++) {
                                                var t = pair.Value[i];
                                                if (t != null) {
                                                        Log.Write("vapre", "TerrainTemplateInfo, tileinfo, terraintype:{0} height:{1} ramptype:{2} leftcol: {3}, rightcol{4}, zoffset:{5}, zramp:{6}\n",
                                                                t.TerrainType,
                                                                t.Height,
                                                                t.RampType,
                                                                t.LeftColor,
                                                                t.RightColor,
                                                                t.ZOffset,
                                                                t.ZRamp
                                                                );
                                                }
                                        }
                                        mapPos = WriteTerrainTemplate(mapPos, map, tileSet, pair.Key); 
                                }
                        };
			menu = widget.Get("MAP_GENERATOR_MAIN_MENU");
			var mpe = world.WorldActor.TraitOrDefault<MenuPaletteEffect>();
			if (mpe != null)
				mpe.Fade(mpe.Info.MenuEffect);

                        /*
			Action onQuit = () =>
			{
				if (world.Type == WorldType.Regular)
					Game.Sound.PlayNotification(world.Map.Rules, null, "Speech", "Leave", world.LocalPlayer == null ? null : world.LocalPlayer.Faction.InternalName);

				leaving = true;

				var iop = world.WorldActor.TraitsImplementing<IObjectivesPanel>().FirstOrDefault();
				var exitDelay = iop != null ? iop.ExitDelay : 0;
				if (mpe != null)
				{
					Game.RunAfterDelay(exitDelay, () =>
					{
						if (Game.IsCurrentWorld(world))
							mpe.Fade(MenuPaletteEffect.EffectType.Black);
					});
					exitDelay += 40 * mpe.Info.FadeLength;
				}

				Game.RunAfterDelay(exitDelay, () =>
				{
					if (!Game.IsCurrentWorld(world))
						return;

					Game.Disconnect();
					Ui.ResetAll();
					Game.LoadShellMap();
				});
			};
                        */

			var buttonTileTraits = menu.Get<ButtonWidget>("MAP_GENERATOR_CLASSIFY_TILES");
                        buttonTileTraits.OnClick = actionShowTileTraitsPanel;
			var buttonGenerate = menu.Get<ButtonWidget>("MAP_GENERATOR_GENERATE");
                        buttonGenerate.OnClick = actionListTileSet;

			//buttonTileTraits.IsVisible = true
			//abortMissionButton.IsDisabled = () => leaving;
			//if (world.IsGameOver)
			//	abortMissionButton.GetText = () => "Leave";
                        /*
			abortMissionButton.OnClick = () =>
			{
				hideMenu = true;

				if (world.LocalPlayer == null || world.LocalPlayer.WinState != WinState.Won)
				{
					Action restartAction = null;
					var iop = world.WorldActor.TraitsImplementing<IObjectivesPanel>().FirstOrDefault();
					var exitDelay = iop != null ? iop.ExitDelay : 0;

					if (world.LobbyInfo.IsSinglePlayer)
					{
						restartAction = () =>
						{
							Ui.CloseWindow();
							if (mpe != null)
							{
								if (Game.IsCurrentWorld(world))
									mpe.Fade(MenuPaletteEffect.EffectType.Black);
								exitDelay += 40 * mpe.Info.FadeLength;
							}

							Game.RunAfterDelay(exitDelay, Game.RestartGame);
						};
					}

					ConfirmationDialogs.PromptConfirmAction(
						title: "Leave Mission",
						text: "Leave this game and return to the menu?",
						onConfirm: onQuit,
						onCancel: showMenu,
						confirmText: "Leave",
						cancelText: "Stay",
						otherText: "Restart",
						onOther: restartAction);
				}
				else
					onQuit();
			};*/
			Action actionCloseMenu = () =>
			{
				Ui.CloseWindow();
				if (mpe != null)
					mpe.Fade(MenuPaletteEffect.EffectType.None);
				onExit();
			};

                
                        //Action actionNone = () => {};

			var buttonMapGeneratorMainMenuExit = menu.Get<ButtonWidget>("MAP_GENERATOR_MAIN_MENU_EXIT");
			buttonMapGeneratorMainMenuExit.OnClick = actionCloseMenu;
			/*{
                                
				ConfirmationDialogs.PromptConfirmAction(
					title: "Exit Map Generator",
					text: "Exit Map Generator?",
					onConfirm: actionCloseMenu,
					onCancel: actionNone);
			};*/
                        /*
			Action onSurrender = () =>
			{
				world.IssueOrder(new Order("Surrender", world.LocalPlayer.PlayerActor, false));
				closeMenu();
			};
			var surrenderButton = menu.Get<ButtonWidget>("SURRENDER");
			surrenderButton.IsVisible = () => world.Type == WorldType.Regular;
			surrenderButton.IsDisabled = () =>
				world.LocalPlayer == null || world.LocalPlayer.WinState != WinState.Undefined ||
				world.Map.Visibility.HasFlag(MapVisibility.MissionSelector) || hasError;
			surrenderButton.OnClick = () =>
			{
				hideMenu = true;
				ConfirmationDialogs.PromptConfirmAction(
					title: "Surrender",
					text: "Are you sure you want to surrender?",
					onConfirm: onSurrender,
					onCancel: showMenu,
					confirmText: "Surrender",
					cancelText: "Stay");
			};

			var saveMapButton = menu.Get<ButtonWidget>("SAVE_MAP");
			saveMapButton.IsVisible = () => world.Type == WorldType.Editor;
			saveMapButton.OnClick = () =>
			{
				hideMenu = true;
				var editorActorLayer = world.WorldActor.Trait<EditorActorLayer>();
				Ui.OpenWindow("SAVE_MAP_PANEL", new WidgetArgs()
				{
					{ "onSave", (Action<string>)(_ => hideMenu = false) },
					{ "onExit", () => hideMenu = false },
					{ "map", world.Map },
					{ "playerDefinitions", editorActorLayer.Players.ToMiniYaml() },
					{ "actorDefinitions", editorActorLayer.Save() }
				});
			};

			var musicButton = menu.Get<ButtonWidget>("MUSIC");
			musicButton.IsDisabled = () => leaving;
			musicButton.OnClick = () =>
			{
				hideMenu = true;
				Ui.OpenWindow("MUSIC_PANEL", new WidgetArgs()
				{
					{ "onExit", () => hideMenu = false },
					{ "world", world }
				});
			};

			var settingsButton = widget.Get<ButtonWidget>("SETTINGS");
			settingsButton.IsDisabled = () => leaving;
			settingsButton.OnClick = () =>
			{
				hideMenu = true;
				Ui.OpenWindow("SETTINGS_PANEL", new WidgetArgs()
				{
					{ "world", world },
					{ "worldRenderer", worldRenderer },
					{ "onExit", () => hideMenu = false },
				});
			};

			var resumeButton = menu.Get<ButtonWidget>("RESUME");
			resumeButton.IsDisabled = () => leaving;
			if (world.IsGameOver)
				resumeButton.GetText = () => "Return to map";

			resumeButton.OnClick = closeMenu;

			var panelRoot = widget.GetOrNull("PANEL_ROOT");
			if (panelRoot != null && world.Type != WorldType.Editor)
			{
				var gameInfoPanel = Game.LoadWidget(world, "GAME_INFO_PANEL", panelRoot, new WidgetArgs()
				{
					{ "activePanel", activePanel }
				});

				gameInfoPanel.IsVisible = () => !hideMenu;
			}
                        */
		}
	}
}
