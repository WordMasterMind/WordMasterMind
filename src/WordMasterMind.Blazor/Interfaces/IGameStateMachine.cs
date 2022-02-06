using WordMasterMind.Blazor.Enumerations;
using WordMasterMind.Library.Enumerations;
using WordMasterMind.Library.Models;

namespace WordMasterMind.Blazor.Interfaces;

public interface IGameStateMachine
{
    public GameState State { get; }
    public bool HardMode { get; set; }
    public WordMasterMindGame? Game { get; }
    public LiteralDictionarySourceType? DictionarySourceType { get; set; }
    public LiteralDictionary? LiteralDictionary { get; }
    public int? WordLength { get; set; }
    public HttpClient? HttpClient { get; set; }
    public Task SetStateAsync(GameState newState);
}