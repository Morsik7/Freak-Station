// Inspired by Nyanotrasen
using Content.Shared.ADT.CharacterFlavor;
using System.Net.Http;
using System.Threading.Tasks;

namespace Content.Server.ADT.CharacterFlavor;

public sealed class CharacterFlavorSystem : SharedCharacterFlavorSystem
{
    private static readonly HttpClient HttpClient = new HttpClient();

    public override void Initialize()
    {
        //Freak Edit Start
        base.Initialize();
        if (HttpClient.DefaultRequestHeaders.UserAgent.Count == 0)
        {
            HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            HttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("image/*"));
        }
        //Freak Edit End
    }

    protected override async void OpenFlavor(EntityUid actor, EntityUid target)
    {
        base.OpenFlavor(actor, target);

        if (!TryComp<CharacterFlavorComponent>(target, out var flavor))
            return;

        if (flavor.HeadshotUrl == string.Empty)
            return;

        var image = await DownloadImageAsync(flavor.HeadshotUrl);

        if (image == null)
            return;

        var ev = new SetHeadshotUiMessage(GetNetEntity(target), image);
        RaiseNetworkEvent(ev, actor);
    }
    //Freak Edit Start
    public async Task<byte[]?> DownloadImageAsync(string url)
    {
        try
        {
            using var response = await HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
            {
                Logger.Error($"[HEADSHOT] Сервер ответил ошибкой {response.StatusCode} для {url}");
                return null;
            }
            var contentType = response.Content.Headers.ContentType?.MediaType;
            if (contentType == null || !contentType.StartsWith("image/"))
            {
                Logger.Error($"[HEADSHOT] ОПАСНОСТЬ: Попытка скачать не картинку ({contentType}) по ссылке {url}");
                return null;
            }
            if (response.Content.Headers.ContentLength > 5 * 1024 * 1024)
            {
                Logger.Error($"[HEADSHOT] Файл слишком большой (>{5}MB): {url}");
                return null;
            }
            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            Logger.Error($"[HEADSHOT] Критическая ошибка при скачивании {url}: {ex.Message}");
            return null;
        }
    }
    //Freak Edit End
}
