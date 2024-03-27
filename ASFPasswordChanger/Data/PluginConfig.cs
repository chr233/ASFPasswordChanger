namespace ASFPasswordChanger.Data;

public sealed record PluginConfig
{
    /// <summary>
    /// 启用统计信息
    /// </summary>
    public bool Statistic { get; set; } = true;
}
