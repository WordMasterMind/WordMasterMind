using Microsoft.AspNetCore.Components;
using WordMasterMind.Library.Enumerations;

namespace WordMasterMind.Blazor.Components;

/// <summary>
///     GameTile is used by GameRow/GameBoard and the Rules splash, but notably rules does not use GameRow.
/// </summary>
public partial class GameTile
{
    [ParameterAttribute] public string Letter { get; set; } = string.Empty;

    [ParameterAttribute] public string Evaluation { get; set; } = Enum.GetName(typeof(LetterEvaluation), LetterEvaluation.Absent)!.ToLowerInvariant();

    public LetterEvaluation LetterEvaluation
    {
        get => (LetterEvaluation) Enum.Parse(
            enumType: typeof(LetterEvaluation),
            value: this.Evaluation,
            ignoreCase: true);
        set => this.Evaluation = value.ToString().ToLowerInvariant();
    }

    [ParameterAttribute] public string? Reveal { get; set; }

    public bool RevealBool => this.Reveal?.ToLowerInvariant() is "true" or "reveal";
}