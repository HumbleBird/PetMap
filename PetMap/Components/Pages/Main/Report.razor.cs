using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using PetMap.Data.Entities;
using PetMap.Data.Utiles;

namespace PetMap.Components.Pages.Main;

public partial class Report : ComponentBase, IDisposable
{
    // 화면 입력값과 검색 결과를 보관한다.
    private PlaceReport report = new PlaceReport(); 
    private bool showReportModal = false;
    private string searchKeyword = "";
    private int imageCount = 0;
    private int imageInputKey = 0;
    private string errorMessage = "";


    private List<KakaoPlaceResult> searchResults = new();
    public class KakaoPlaceResult
    {
        public string Name { get; set; } = "";
        public string RoadAddress { get; set; } = "";
        public string JibunAddress { get; set; } = "";

        public string PlaceUrl { get; set; } = "";

        public string BusinessHours { get; set; } = "";

        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    private DotNetObjectReference<Report>? reportReference;

    private HashSet<string> selectedCategories = new();
    public const int MaxSelectedCategories = 3;

    public bool ShowCloseReportModal = false;

    // 창을 열기 전에 브라우저에 저장된 임시 제보가 있으면 복원한다.
    private async Task OpenReportModal()
    {
        var draftJson = await LoadTempReportAsync();
        if (!string.IsNullOrWhiteSpace(draftJson))
        {
            var draft = System.Text.Json.JsonSerializer.Deserialize<ReportDraft>(
                draftJson,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (draft is not null)
            {
                report = draft.Report ?? new PlaceReport();
                selectedCategories = new HashSet<string>(draft.SelectedCategories ?? []);
                searchKeyword = draft.SearchKeyword ?? report.PlaceName;
                searchResults.Clear();
            }
        }

        showReportModal = true;
    }

    private void CloseReportModal()
    {
        showReportModal = false;
    }

    private void ShowCheckCloseReportModal()
    {
        ShowCloseReportModal = true;
    }

    private void ContinueWriting()
    {
        ShowCloseReportModal = false;
    }

    // 작성한 내용을 제보 테이블에 등록하고 저장 초안과 화면 상태를 정리한다.
    private async Task SubmitReport()
    {
        var reportData = new PlaceReport
        {
            Nickname = report.Nickname,
            PlaceName = report.PlaceName,
            Category = string.Join(", ", selectedCategories),

            Latitude = report.Latitude,
            Longitude = report.Longitude,

            NaverPlaceUrl = report.NaverPlaceUrl,
            BusinessHours = report.BusinessHours,

            Review = report.Review,
            Memo = report.Memo,

            Status = Status.Pending,
        };

        Db.PlaceReports.Add(reportData);

        await Db.SaveChangesAsync();

        await ClearTempReportAsync();
        ResetReportForm();
        ShowCloseReportModal = false;

        CloseReportModal();
    }

    private void HandleImages(InputFileChangeEventArgs e)
    {
        imageCount = e.GetMultipleFiles().Count();

        if (imageCount < 2)
        {
            errorMessage = "이미지는 최소 2장 등록해야 합니다.";
            return;
        }
    }

    // 장소 검색 결과를 등록하는 기존 Javascript 검색 브리지에 요청한다.
    private async Task SearchPlace()
    {
        if (string.IsNullOrWhiteSpace(searchKeyword))
        {
            searchResults.Clear();
            return;
        }

        await JS.InvokeVoidAsync(
            "petMap.searchPlaces",
            searchKeyword,
            reportReference
        );
    }

    private void SelectPlace(KakaoPlaceResult result)
    {
        report.PlaceName = result.Name;
        report.Latitude = result.Latitude;
        report.Longitude = result.Longitude;
        report.NaverPlaceUrl = result.PlaceUrl;
        report.BusinessHours = result.BusinessHours;
        searchResults.Clear();
        searchKeyword = result.Name;
    }

    protected override void OnInitialized()
    {
        reportReference = DotNetObjectReference.Create(this);
    }

    // Kakao 검색 Javascript가 검색 결과를 다시 C#으로 전달할 때 호출된다.
    [JSInvokable]
    public async Task ReceiveSearchResults(List<KakaoPlaceResult> results)
    {
        searchResults = results;

        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        reportReference?.Dispose();
    }

    private void ToggleCategory(string category)
    {
        if (selectedCategories.Contains(category))
        {
            selectedCategories.Remove(category);
        }
        else
        {
            if (selectedCategories.Count >= MaxSelectedCategories)
                return;

            selectedCategories.Add(category);
        }

        StateHasChanged();
    }

    // 저장 안 함을 선택하면 초안과 입력 상태를 모두 지운다.
    private async Task NotSaveTempReport()
    {
        await ClearTempReportAsync();
        ResetReportForm();
        ShowCloseReportModal = false;
        CloseReportModal();
    }

    // 입력값과 카테고리를 브라우저 로컬 저장소에 임시 보관한다.
    private async Task SaveTempReport()
    {
        var draft = new ReportDraft
        {
            Report = report,
            SelectedCategories = selectedCategories.ToList(),
            SearchKeyword = searchKeyword
        };
        var draftJson = System.Text.Json.JsonSerializer.Serialize(draft);
        await InvokeDraftStorageAsync("saveReportDraft", draftJson);

        ShowCloseReportModal = false;
        CloseReportModal();
    }

    // 새 제보를 시작할 수 있도록 입력, 검색 결과, 파일 입력을 초기화한다.
    private void ResetReportForm()
    {
        report = new PlaceReport();
        selectedCategories.Clear();
        searchKeyword = "";
        searchResults.Clear();
        imageCount = 0;
        errorMessage = "";
        imageInputKey++;
    }

    // 임시저장 모듈을 불러와 저장된 JSON 초안을 가져온다.
    private async Task<string?> LoadTempReportAsync()
    {
        var module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/src/reportDraft.js");
        try
        {
            return await module.InvokeAsync<string?>("loadReportDraft");
        }
        finally
        {
            await module.DisposeAsync();
        }
    }

    private async Task ClearTempReportAsync()
    {
        await InvokeDraftStorageAsync("clearReportDraft");
    }

    // 임시저장 모듈의 저장 또는 삭제 함수를 호출한다.
    private async Task InvokeDraftStorageAsync(string method, string? json = null)
    {
        var module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/src/reportDraft.js");
        try
        {
            if (json is null)
                await module.InvokeVoidAsync(method);
            else
                await module.InvokeVoidAsync(method, json);
        }
        finally
        {
            await module.DisposeAsync();
        }
    }

    private sealed class ReportDraft
    {
        public PlaceReport? Report { get; set; }
        public List<string>? SelectedCategories { get; set; }
        public string? SearchKeyword { get; set; }
    }
}

