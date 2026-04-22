// SPDX-FileCopyrightText: 2024 Your Name <you@example.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.ResourceManagement;
using static Robust.Client.UserInterface.StylesheetHelpers;

namespace Content.Client.Stylesheets
{
    public sealed class YellowButtonStyleSheet
    {
        public Stylesheet Stylesheet { get; }

        public YellowButtonStyleSheet(IResourceCache resCache)
        {
            Stylesheet = new Stylesheet(CreateRules());
        }

        private List<StyleRule> CreateRules()
        {
            return new List<StyleRule>
            {
                // Основная желтая кнопка
                Element<Button>().Class(YellowButtonStyles.StyleClassButtonColorYellow)
                    .Prop(Control.StylePropertyModulateSelf, YellowButtonStyles.ButtonColorDefaultYellow),

                Element<Button>().Class(YellowButtonStyles.StyleClassButtonColorYellow)
                    .Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(Control.StylePropertyModulateSelf, YellowButtonStyles.ButtonColorDefaultYellow),

                Element<Button>().Class(YellowButtonStyles.StyleClassButtonColorYellow)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, YellowButtonStyles.ButtonColorHoveredYellow),

                Element<Button>().Class(YellowButtonStyles.StyleClassButtonColorYellow)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Control.StylePropertyModulateSelf, YellowButtonStyles.ButtonColorPressedYellow),

                Element<Button>().Class(YellowButtonStyles.StyleClassButtonColorYellow)
                    .Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Control.StylePropertyModulateSelf, YellowButtonStyles.ButtonColorDisabledYellow),

                // Текст для желтых кнопок (черный для лучшей читаемости)
                new StyleRule(
                    new SelectorChild(
                        new SelectorElement(typeof(Button), new[] { YellowButtonStyles.StyleClassButtonColorYellow }, null, null),
                        new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty("font-color", YellowButtonStyles.ButtonColorTextYellow)
                    }),

                // Яркая желтая кнопка
                Element<Button>().Class(YellowButtonStyles.StyleClassButtonColorYellowBright)
                    .Prop(Control.StylePropertyModulateSelf, YellowButtonStyles.ButtonColorDefaultYellowBright),

                Element<Button>().Class(YellowButtonStyles.StyleClassButtonColorYellowBright)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, YellowButtonStyles.ButtonColorHoveredYellowBright),

                // Темная желтая кнопка
                Element<Button>().Class(YellowButtonStyles.StyleClassButtonColorYellowDark)
                    .Prop(Control.StylePropertyModulateSelf, YellowButtonStyles.ButtonColorDefaultYellowDark),

                Element<Button>().Class(YellowButtonStyles.StyleClassButtonColorYellowDark)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, YellowButtonStyles.ButtonColorHoveredYellowDark),

                // Предупреждающая желтая кнопка
                Element<Button>().Class(YellowButtonStyles.StyleClassButtonColorYellowCaution)
                    .Prop(Control.StylePropertyModulateSelf, YellowButtonStyles.ButtonColorDefaultYellowCaution),

                Element<Button>().Class(YellowButtonStyles.StyleClassButtonColorYellowCaution)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, YellowButtonStyles.ButtonColorHoveredYellowCaution),
            };
        }
    }
}