// SPDX-FileCopyrightText: 2026 Freaky Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.UserInterface.Controls;
using Content.Shared._FreakyStation.Psionics;
using Robust.Client.UserInterface.Controls;
using System;
using System.Numerics;

namespace Content.Client._FreakyStation.Psionics;

public sealed class PsionicStudyWindow : FancyWindow
{
    public Action<PsionicAbility>? OnToggleAbility;

    private readonly Label _summaryLabel;
    private readonly BoxContainer _entriesContainer;

    public PsionicStudyWindow()
    {
        Title = "Psionic Study";
        MinSize = new Vector2(460, 380);

        var root = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 6,
        };

        _summaryLabel = new Label
        {
            Text = string.Empty,
        };

        var scroll = new ScrollContainer
        {
            VerticalExpand = true,
            HorizontalExpand = true,
        };

        _entriesContainer = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 6,
            HorizontalExpand = true,
        };

        scroll.AddChild(_entriesContainer);
        root.AddChild(_summaryLabel);
        root.AddChild(scroll);
        AddChild(root);
    }

    public void UpdateState(PsionicStudyBoundUserInterfaceState state)
    {
        _summaryLabel.Text = $"Learned: {state.LearnedCount}/{state.MaxLearned}";
        _entriesContainer.Children.Clear();

        foreach (var entry in state.Abilities)
        {
            var canToggle = entry.Learned
                ? state.LearnedCount > 1
                : state.LearnedCount < state.MaxLearned;

            var row = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Vertical,
                SeparationOverride = 2,
            };

            var button = new Button
            {
                Text = $"{(entry.Learned ? "[Learned] " : "[Unlearned] ")}{entry.Name}",
                HorizontalExpand = true,
                Disabled = !canToggle,
            };
            button.OnPressed += _ => OnToggleAbility?.Invoke(entry.Ability);

            var description = new Label
            {
                Text = entry.Description,
            };

            row.AddChild(button);
            row.AddChild(description);
            _entriesContainer.AddChild(row);
        }
    }
}
