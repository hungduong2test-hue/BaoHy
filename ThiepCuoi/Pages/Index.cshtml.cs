using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text;

namespace ThiepCuoi.Pages;

public class IndexModel : PageModel
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        IOptionsSnapshot<WeddingPageData> weddingOptions,
        IWebHostEnvironment environment,
        ILogger<IndexModel> logger)
    {
        Wedding = weddingOptions.Value;
        _environment = environment;
        _logger = logger;
    }

    public WeddingPageData Wedding { get; }

    [BindProperty]
    public string Guest { get; set; } = string.Empty;

    [BindProperty]
    public string Attending { get; set; } = string.Empty;

    [BindProperty]
    public string Message { get; set; } = string.Empty;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostSubmitRsvpAsync()
    {
        Guest = Guest.Trim();
        Message = Message.Trim();

        if (string.IsNullOrWhiteSpace(Guest) ||
            Guest.Length > 200 ||
            Message.Length == 0 ||
            Message.Length > 2000 ||
            (Attending != "Có, tôi sẽ đến chung vui" &&
             Attending != "Rất tiếc, tôi chưa thể tham dự"))
        {
            return new JsonResult(new
            {
                success = false,
                message = "Vui lòng kiểm tra và nhập đầy đủ thông tin phản hồi."
            })
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        var record = string.Join(
            " | ",
            DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture),
            FormatRsvpField(Guest),
            FormatRsvpField(Attending),
            FormatRsvpField(Message)) + Environment.NewLine;

        try
        {
            var filePath = Path.Combine(_environment.ContentRootPath, "Comfirm.txt");
            await System.IO.File.AppendAllTextAsync(
                filePath,
                record,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                HttpContext.RequestAborted);
        }
        catch (IOException exception)
        {
            _logger.LogError(exception, "Unable to append the RSVP response to Comfirm.txt.");
            return new JsonResult(new
            {
                success = false,
                message = "Chưa thể lưu phản hồi. Vui lòng thử lại sau."
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
        catch (UnauthorizedAccessException exception)
        {
            _logger.LogError(exception, "The application is not allowed to write Comfirm.txt.");
            return new JsonResult(new
            {
                success = false,
                message = "Chưa thể lưu phản hồi. Vui lòng thử lại sau."
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        return new JsonResult(new
        {
            success = true,
            message = $"Cảm ơn {Guest}, phản hồi của bạn đã được lưu."
        });
    }

    private static string FormatRsvpField(string value)
    {
        return value
            .Replace("\r\n", " ", StringComparison.Ordinal)
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Replace('\t', ' ')
            .Replace("|", "/", StringComparison.Ordinal);
    }
}

public sealed class WeddingPageData
{
    public string BrideName { get; set; } = "Thị Hoàng";
    public string GroomName { get; set; } = "Văn Hùng";
    public string BrideFamily { get; set; } = "Nhà gái";
    public string GroomFamily { get; set; } = "Nhà trai";
    public string BrideParents { get; set; } = "Gia đình cô dâu";
    public string GroomParents { get; set; } = "Gia đình chú rể";
    public string GroomFatherName { get; set; } = "Lê Hồng Long";
    public string GroomMotherName { get; set; } = "Nguyễn Thị Minh Liễu";
    public string GroomHomeAddress { get; set; } = "KDC 20, TDP Lò Bàn, Phường Đức Phổ, Tỉnh Quảng Ngãi";
    public string BrideFatherName { get; set; } = "Dương Văn Lựa";
    public string BrideMotherName { get; set; } = "Nguyễn Thị Yến";
    public string BrideHomeAddress { get; set; } = "TDP Thành Long, Phường Yên Dũng, TP Bắc Ninh";
    public string BrideRank { get; set; } = "ÚT NỮ";
    public string GroomRank { get; set; } = "QUÝ NAM";
    public string InvitationType { get; set; } = "Thành Hôn";
    public string CeremonyAnnouncement { get; set; } = "LỄ THÀNH HÔN CỦA CON CHÚNG TÔI";
    public string CeremonyLocationText { get; set; } = "LỄ THÀNH HÔN ĐƯỢC CỬ HÀNH TẠI";
    public string CeremonyPlace { get; set; } = "TƯ GIA";
    public string CeremonyFamilyOrder { get; set; } = "GroomFirst";
    public string LunarDateText { get; set; } = "( TỨC NGÀY 11/04 NĂM BÍNH NGỌ )";
    public DateTime WeddingDate { get; set; } = new(2026, 5, 27, 10, 0, 0);
    public string DateLabel { get; set; } = "Thứ Tư, 27 tháng 5, 2026";
    public string Venue { get; set; } = "Tư gia hai họ";
    public string Address { get; set; } = "6 Đ. Bờ Ruộng, Đức Phổ, Quảng Ngãi, Việt Nam";
    public string MapImage { get; set; } = "/images/wedding/map-preview.png";
    public string HeroImage { get; set; } = "/images/trang-dau/chu-re.jpg";
    public string PortraitImage { get; set; } = "/images/trang-dau/co-dau.jpg";
    public string InvitationText { get; set; } = "Trân trọng kính mời quý khách đến chung vui trong ngày thành hôn của chúng tôi. Sự hiện diện của quý khách là niềm vinh hạnh cho gia đình hai họ.";
    public int AutoScrollSpeed { get; set; } = 145;
    public int AutoScrollDelay { get; set; } = 400;
    public List<HeroProfile> HeroProfiles { get; set; } = [];
    public List<CeremonyFamily> CeremonyFamilies { get; set; } = [];
    public List<PersonProfile> CoupleProfiles { get; set; } = [];
    public List<EventDetail> Events { get; set; } = [];
    public List<TimelineItem> Timeline { get; set; } = [];
    public List<GalleryImage> Gallery { get; set; } = [];
    public List<BankAccountInfo> BankAccounts { get; set; } = [];
    public List<WishItem> Wishes { get; set; } = [];

    public string MapQuery => Uri.EscapeDataString($"{Venue}, {Address}");
    public string MapUrl => $"https://www.google.com/maps/dir/?api=1&destination={MapQuery}";
    public string MapEmbedUrl => $"https://maps.google.com/maps?hl=vi&q={MapQuery}&z=16&ie=UTF8&iwloc=B&output=embed";
    public IReadOnlyList<BankAccountInfo> ValidBankAccounts => BankAccounts
        .Where(account => CoupleProfiles.Any(profile => NamesMatch(profile.Name, account.AccountHolder)))
        .ToList();
    public IReadOnlyList<HeroProfile> DisplayHeroProfiles => HeroProfiles.Count > 0
        ? OrderHeroProfiles(HeroProfiles)
        : OrderHeroProfiles([
            new HeroProfile { Role = BrideRank, Name = BrideName, Image = PortraitImage },
            new HeroProfile { Role = GroomRank, Name = GroomName, Image = HeroImage }
        ]);
    public IReadOnlyList<CeremonyFamily> DisplayCeremonyFamilies => CeremonyFamilies.Count > 0
        ? OrderCeremonyFamilies(CeremonyFamilies)
        : OrderCeremonyFamilies([
            new CeremonyFamily { Family = GroomFamily, FatherName = GroomFatherName, MotherName = GroomMotherName, Address = GroomHomeAddress },
            new CeremonyFamily { Family = BrideFamily, FatherName = BrideFatherName, MotherName = BrideMotherName, Address = BrideHomeAddress }
        ]);
    public IReadOnlyList<PersonProfile> OrderedCoupleProfiles
    {
        get
        {
            if (CoupleProfiles.Count == 0 || DisplayHeroProfiles.Count == 0)
            {
                return CoupleProfiles;
            }

            var ordered = new List<PersonProfile>();

            foreach (var heroProfile in DisplayHeroProfiles)
            {
                var match = CoupleProfiles.FirstOrDefault(profile =>
                    !ordered.Contains(profile) && NamesReferToSamePerson(profile.Name, heroProfile.Name));

                if (match is not null)
                {
                    ordered.Add(match);
                }
            }

            ordered.AddRange(CoupleProfiles.Where(profile => !ordered.Contains(profile)));

            return ordered;
        }
    }
    public IReadOnlyList<BankAccountInfo> OrderedValidBankAccounts
    {
        get
        {
            var validAccounts = ValidBankAccounts.ToList();

            if (validAccounts.Count == 0 || OrderedCoupleProfiles.Count == 0)
            {
                return validAccounts;
            }

            var ordered = new List<BankAccountInfo>();

            foreach (var profile in OrderedCoupleProfiles)
            {
                ordered.AddRange(validAccounts.Where(account =>
                    !ordered.Contains(account) && NamesMatch(profile.Name, account.AccountHolder)));
            }

            ordered.AddRange(validAccounts.Where(account => !ordered.Contains(account)));

            return ordered;
        }
    }

    private static bool NamesMatch(string profileName, string accountHolder)
    {
        return NormalizeName(profileName) == NormalizeName(accountHolder);
    }

    private static bool NamesReferToSamePerson(string left, string right)
    {
        var normalizedLeft = NormalizeName(left);
        var normalizedRight = NormalizeName(right);

        return normalizedLeft == normalizedRight ||
            normalizedLeft.Contains(normalizedRight, StringComparison.Ordinal) ||
            normalizedRight.Contains(normalizedLeft, StringComparison.Ordinal);
    }

    private IReadOnlyList<HeroProfile> OrderHeroProfiles(IEnumerable<HeroProfile> profiles)
    {
        var orderedProfiles = profiles.ToList();

        if (orderedProfiles.Count < 2)
        {
            return orderedProfiles;
        }

        var groomProfile = orderedProfiles.FirstOrDefault(IsGroomProfile);
        var brideProfile = orderedProfiles.FirstOrDefault(IsBrideProfile);

        if (groomProfile is null || brideProfile is null)
        {
            return orderedProfiles;
        }

        var firstProfile = CeremonyFamilyOrder.Trim().Equals("BrideFirst", StringComparison.OrdinalIgnoreCase)
            ? brideProfile
            : groomProfile;
        var secondProfile = ReferenceEquals(firstProfile, brideProfile) ? groomProfile : brideProfile;

        return [firstProfile, secondProfile, .. orderedProfiles.Where(profile => !ReferenceEquals(profile, firstProfile) && !ReferenceEquals(profile, secondProfile))];
    }

    private IReadOnlyList<CeremonyFamily> OrderCeremonyFamilies(IEnumerable<CeremonyFamily> families)
    {
        var orderedFamilies = families.ToList();

        if (orderedFamilies.Count < 2)
        {
            return orderedFamilies;
        }

        var groomFamily = orderedFamilies.FirstOrDefault(family => IsGroomFamily(family.Family));
        var brideFamily = orderedFamilies.FirstOrDefault(family => IsBrideFamily(family.Family));

        if (groomFamily is null || brideFamily is null)
        {
            return orderedFamilies;
        }

        var firstFamily = CeremonyFamilyOrder.Trim().Equals("BrideFirst", StringComparison.OrdinalIgnoreCase)
            ? brideFamily
            : groomFamily;
        var secondFamily = ReferenceEquals(firstFamily, brideFamily) ? groomFamily : brideFamily;

        return [firstFamily, secondFamily, .. orderedFamilies.Where(family => !ReferenceEquals(family, firstFamily) && !ReferenceEquals(family, secondFamily))];
    }

    private static bool IsGroomFamily(string value)
    {
        return NormalizeName(value).Contains("NHATRAI", StringComparison.Ordinal);
    }

    private static bool IsBrideFamily(string value)
    {
        return NormalizeName(value).Contains("NHAGAI", StringComparison.Ordinal);
    }

    private bool IsGroomProfile(HeroProfile profile)
    {
        return NamesReferToSamePerson(profile.Name, GroomName) ||
            NormalizeName(profile.Role).Contains("NAM", StringComparison.Ordinal) ||
            profile.Image.Contains("chu-re", StringComparison.OrdinalIgnoreCase);
    }

    private bool IsBrideProfile(HeroProfile profile)
    {
        return NamesReferToSamePerson(profile.Name, BrideName) ||
            NormalizeName(profile.Role).Contains("NU", StringComparison.Ordinal) ||
            profile.Image.Contains("co-dau", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeName(string value)
    {
        var normalized = value.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark && !char.IsWhiteSpace(character))
            {
                builder.Append(char.ToUpperInvariant(character));
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}

public sealed class HeroProfile
{
    public string Role { get; set; } = "";
    public string Name { get; set; } = "";
    public string Image { get; set; } = "";
}

public sealed class CeremonyFamily
{
    public string Family { get; set; } = "";
    public string FatherName { get; set; } = "";
    public string MotherName { get; set; } = "";
    public string Address { get; set; } = "";
}

public sealed class TimelineItem
{
    public string Time { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
}

public sealed class PersonProfile
{
    public string Role { get; set; } = "";
    public string Name { get; set; } = "";
    public string Parents { get; set; } = "";
    public string Address { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Image { get; set; } = "";
    public string Description { get; set; } = "";
}

public sealed class EventDetail
{
    public string Title { get; set; } = "";
    public string Family { get; set; } = "";
    public string Date { get; set; } = "";
    public string Time { get; set; } = "";
    public string Address { get; set; } = "";
    public string Description { get; set; } = "";
}

public sealed class GalleryImage
{
    public string Url { get; set; } = "";
    public string Caption { get; set; } = "";
}

public sealed class WishItem
{
    public string Name { get; set; } = "";
    public string Message { get; set; } = "";
}

public sealed class BankAccountInfo
{
    public string Role { get; set; } = "";
    public string BankCode { get; set; } = "";
    public string BankName { get; set; } = "";
    public string AccountNumber { get; set; } = "";
    public string AccountHolder { get; set; } = "";
    public string TransferNote { get; set; } = "Mung cuoi";
    public string QrUrl => $"https://img.vietqr.io/image/{Uri.EscapeDataString(BankCode)}-{Uri.EscapeDataString(AccountNumber)}-compact2.png?addInfo={Uri.EscapeDataString(TransferNote)}&accountName={Uri.EscapeDataString(AccountHolder)}";
}
