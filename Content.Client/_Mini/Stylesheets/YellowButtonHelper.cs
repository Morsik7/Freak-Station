// SPDX-FileCopyrightText: 2024 Your Name <you@example.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.UserInterface.Controls;
using Content.Client.Stylesheets;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Placement;
using Robust.Shared.Maths;
using System.Linq;
using System.Numerics;
using Content.Client.Lobby;
using Content.Client.UserInterface.Systems.MenuBar.Widgets;
using Content.Shared.Construction.Prototypes;
using Content.Shared.Whitelist;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Shared.Enums;

namespace Content.Client.UserInterface.Controls
{
    public static class YellowButtonHelper
    {
        /// <summary>
        /// Делает кнопку желтой
        /// </summary>
        public static void MakeYellow(this Button button, string variant = "default")
        {
            button.RemoveStyleClass(YellowButtonStyles.StyleClassButtonColorYellow);
            button.RemoveStyleClass(YellowButtonStyles.StyleClassButtonColorYellowBright);
            button.RemoveStyleClass(YellowButtonStyles.StyleClassButtonColorYellowDark);
            button.RemoveStyleClass(YellowButtonStyles.StyleClassButtonColorYellowCaution);

            switch (variant.ToLower())
            {
                case "bright":
                    button.AddStyleClass(YellowButtonStyles.StyleClassButtonColorYellowBright);
                    break;
                case "dark":
                    button.AddStyleClass(YellowButtonStyles.StyleClassButtonColorYellowDark);
                    break;
                case "caution":
                    button.AddStyleClass(YellowButtonStyles.StyleClassButtonColorYellowCaution);
                    break;
                default:
                    button.AddStyleClass(YellowButtonStyles.StyleClassButtonColorYellow);
                    break;
            }
        }

        /// <summary>
        /// Создает новую желтую кнопку
        /// </summary>
        public static Button CreateYellowButton(string text, string variant = "default", int width = 125, int height = 10)
        {
            var button = new Button
            {
                Text = text,
                MinSize = new Vector2(width, height) // Исправлено: используем Vector2 вместо tuple
            };

            button.MakeYellow(variant);
            return button;
        }

        /// <summary>
        /// Проверяет, является ли кнопка желтой
        /// </summary>
        public static bool IsYellow(this Button button)
        {
            return button.HasStyleClass(YellowButtonStyles.StyleClassButtonColorYellow) ||
                   button.HasStyleClass(YellowButtonStyles.StyleClassButtonColorYellowBright) ||
                   button.HasStyleClass(YellowButtonStyles.StyleClassButtonColorYellowDark) ||
                   button.HasStyleClass(YellowButtonStyles.StyleClassButtonColorYellowCaution);
        }
    }
}
