using Microsoft.AspNetCore.Components;
using Platform.API.Models;

namespace Platform.SDK.Components.BibleComponents.Verses;

public partial class VerseComponent : ComponentBase
{
    [Parameter, EditorRequired]
    public Passage Passage { get; set; } = default!;
    
    [Parameter]
    public string? Copyright { get; set; }
    
    
    
}