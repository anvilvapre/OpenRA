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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets.Logic
{
	public class MapGeneratorMenuButtonLogic: ChromeLogic
	{
                private MenuButtonWidget buttonMapGenerator;
                //private Widget currentWindow;
                //private Func<bool> currentWindowIsVisible;

		[ObjectCreator.UseCtor]
		public MapGeneratorMenuButtonLogic(Widget widget, World world, ModData modData)
		{
                        var containerEditorWorldRoot = Ui.Root.Get("EDITOR_WORLD_ROOT");
                        buttonMapGenerator = (MenuButtonWidget) widget;
                        Action actionOnExit = () => {
                                { 
                                        containerEditorWorldRoot.IsVisible = () => true;
                                } 
                        };
                        Action actionOpenMapGeneratorMenu = () => { 
                                containerEditorWorldRoot.IsVisible = () => false;
		                Console.WriteLine("Current Widget " + containerEditorWorldRoot.Id);
                                Game.OpenWindow("MAP_GENERATOR_MAIN_MENU", new WidgetArgs()
				{
					//{ "onSelect", onSelect },
					{ "onExit", actionOnExit }
				});
                        };
			buttonMapGenerator.OnClick = actionOpenMapGeneratorMenu;
			buttonMapGenerator.OnKeyPress = e =>
				{
					var key = Hotkey.FromKeyInput(e);
					if (key != Game.Settings.Keys.Production08Key)
						return;
                                        actionOpenMapGeneratorMenu();
				};

                        /*      			// Map editor menu
			var mapEditorMenu = widget.Get("MAP_EDITOR_MENU");
			mapEditorMenu.IsVisible = () => menuType == MenuType.MapEditor;

			var onSelect = new Action<string>(uid =>
			{
				RemoveShellmapUI();
				LoadMapIntoEditor(modData.MapCache[uid].Uid);
			});

			var newMapButton = widget.Get<ButtonWidget>("NEW_MAP_BUTTON");
			newMapButton.OnClick = () =>
			{
				SwitchMenu(MenuType.None);
				Game.OpenWindow("NEW_MAP_BG", new WidgetArgs()
				{
					{ "onSelect", onSelect },
					{ "onExit", () => SwitchMenu(MenuType.MapEditor) }
				});
			};

			var loadMapButton = widget.Get<ButtonWidget>("LOAD_MAP_BUTTON");
			loadMapButton.OnClick = () =>
			{
				SwitchMenu(MenuType.None);
				Game.OpenWindow("MAPCHOOSER_PANEL", new WidgetArgs()
				{
					{ "initialMap", null },
					{ "initialTab", MapClassification.User },
					{ "onExit", () => SwitchMenu(MenuType.MapEditor) },
					{ "onSelect", onSelect },
					{ "filter", MapVisibility.Lobby | MapVisibility.Shellmap | MapVisibility.MissionSelector },
				});
			};

			mapEditorMenu.Get<ButtonWidget>("BACK_BUTTON").OnClick = () => SwitchMenu(MenuType.Extras);

			var newsBG = widget.GetOrNull("NEWS_BG");
			if (newsBG != null)
			{
				newsBG.IsVisible = () => Game.Settings.Game.FetchNews && menuType != MenuType.None;

				newsPanel = Ui.LoadWidget<ScrollPanelWidget>("NEWS_PANEL", null, new WidgetArgs());
				newsTemplate = newsPanel.Get("NEWS_ITEM_TEMPLATE");
				newsPanel.RemoveChild(newsTemplate);

				newsStatus = newsPanel.Get<LabelWidget>("NEWS_STATUS");
				SetNewsStatus("Loading news");

				var cacheFile = Platform.ResolvePath("^", "news.yaml");
				var currentNews = ParseNews(cacheFile);
				if (currentNews != null)
					DisplayNews(currentNews);

				var newsButton = newsBG.GetOrNull<DropDownButtonWidget>("NEWS_BUTTON");

				if (newsButton != null)
				{
					if (!fetchedNews)
						new Download(Game.Settings.Game.NewsUrl + SysInfoQuery(), cacheFile, e => { },
							(e, c) => NewsDownloadComplete(e, cacheFile, currentNews,
							() => newsButton.AttachPanel(newsPanel)));

					newsButton.OnClick = () => newsButton.AttachPanel(newsPanel);
				}
			}

			Game.OnRemoteDirectConnect += OnRemoteDirectConnect;
                        */
		}
}

}
