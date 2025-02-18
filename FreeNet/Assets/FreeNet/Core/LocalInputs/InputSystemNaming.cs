using System;
using UnityEngine.InputSystem;
using static Codice.CM.WorkspaceServer.DataStore.IncomingChanges.StoreIncomingChanges.FileConflicts;
using static UnityEngine.InputSystem.Controls.AxisControl;
using UnityEngine.UIElements;
using UnityEngine;
public static class InputSystemNaming
{
    public enum Device
    {
        Keyboard,
        Mouse
    }
    public enum MouseType
    {
        Pointer,
        delta,
        scroll,
        leftButton,
        rightButton,
        middleButton,
        forwardButton,
        backButton
    }

    public enum Processor
    {
        Clamp,
        Invert,
        Normalize,
        Scale,
        ScaleVector2,
    }


    public enum CompositeType
    {
        Axis1D,
        Vector2D,
        ButtonWithOneModifier,
        ButtonWithTwoModifiers,
    }
    public enum Vector2DSyntax 
    {
        Up,
        Down,
        Left,
        Right
    }
    public enum Axis1DSyntax
    {
        Positive,
        Negative
    }
    public enum ButtonWithModifierSyntax
    {
        Modifier1,
        Modifier2,
        Button
    }



    public static string ToInputSystemName(this Device device)
    {
        return device switch
        {
            Device.Keyboard => "<Keyboard>",
            Device.Mouse => "<Mouse>",
            _ => throw new ArgumentOutOfRangeException(nameof(device), device, null)
        };
    }
    public static string ToInputSystemName(this CompositeType type)
    {
        return type switch
        {
            CompositeType.Axis1D => "1DAxis",
            CompositeType.Vector2D => "2DVector",
            CompositeType.ButtonWithOneModifier => "ButtonWithOneModifier",
            CompositeType.ButtonWithTwoModifiers => "ButtonWithTwoModifiers",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
    public static string ToInputSystemName(this Vector2DSyntax syntax)
    {
        return syntax switch
        {
            Vector2DSyntax.Up => "Up",
            Vector2DSyntax.Down => "Down",
            Vector2DSyntax.Left => "Left",
            Vector2DSyntax.Right => "Right",
            _ => throw new ArgumentOutOfRangeException(nameof(syntax), syntax, null)
        };
    }
    public static string ToInputSystemName(this Axis1DSyntax syntax)
    {
        return syntax switch
        {
            Axis1DSyntax.Negative => "Negative",
            Axis1DSyntax.Positive => "Positive",
            _ => throw new ArgumentOutOfRangeException(nameof(syntax), syntax, null)
        };
    }
    public static string ToInputSystemName(this ButtonWithModifierSyntax syntax)
    {
        return syntax switch
        {
            ButtonWithModifierSyntax.Modifier2 => "Modifier2",
            ButtonWithModifierSyntax.Modifier1 => "Modifier1",
            ButtonWithModifierSyntax.Button => "Button",
            _ => throw new ArgumentOutOfRangeException(nameof(syntax), syntax, null)
        };
    }
    public static string ToInputSystemName(this Key key)
    {
        return key.ToString();
    }
    public static string ToInputSystemName(this MouseType type)
    {
        return type switch
        {
            MouseType.Pointer => "Pointer",
            MouseType.delta => "delta",
            MouseType.scroll => "scroll",
            MouseType.leftButton => "leftButton",
            MouseType.rightButton => "rightButton",
            MouseType.middleButton => "middleButton",
            MouseType.forwardButton => "forwardButton",
            MouseType.backButton => "backButton",

            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }


    public static string ToInputSystemName(this Processor processor)
    {
        return processor switch
        {
            Processor.Invert => "invert",
            Processor.Normalize => "normalize",

            _ => throw new ArgumentOutOfRangeException(nameof(processor), processor, null)
        };
    }

    public static string ToInputSystemName(this Processor processor,float val)
    {
        return processor switch
        {
            Processor.Scale => $"scale(factor={val})",
            _ => throw new ArgumentOutOfRangeException(nameof(processor), processor, null)
        };
    }
    public static string ToInputSystemName(this Processor processor, Vector2 vector)
    {
        return processor switch
        {
            Processor.ScaleVector2 => $"scaleVector2(x={vector.x},y={vector.y})",
            _ => throw new ArgumentOutOfRangeException(nameof(processor), processor, null)
        };
    }
    public static string ToInputSystemName(this Processor processor, float min, float max)
    {
        return processor switch
        {
            Processor.Clamp => $"Clamp(min={min}, max={max})",
            _ => throw new ArgumentOutOfRangeException(nameof(processor), processor, null)
        };
    }
}
