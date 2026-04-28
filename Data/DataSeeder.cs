using Microsoft.EntityFrameworkCore;
using Work_Dashboard.Data.Entities;

namespace Work_Dashboard.Data;

public static class DataSeeder
{
    // Compact seed record
    private record W(
        string? No, string Name, string Jen,
        decimal? San, decimal? Agr,
        string? St, string? En,
        decimal? Ph, decimal? Fin,
        string? Sc, string? Dept, string? Fund, string? Cm,
        string? Cat, string? Firm, string? Mob,
        WorkStatus Status = WorkStatus.ONGOING,
        bool Annual = false,
        decimal? Exp = null,
        string? PaySt = null
    );

    // ── Category normalisation ────────────────────────────────────
    private static string NormCat(string s) => s.Trim().ToUpperInvariant() switch
    {
        "DRAINAGE" or "DRAIN" or "Drain" or "DRAIN " or "Drain " => "DRAIN",
        "ROAD" or "ROAD " or "Road"                              => "ROAD",
        "BUILDING" or "Building"                                  => "BUILDING",
        "ELECTRIC" or "LIGHT" or "Electric" or "LIGHT "          => "ELECTRIC",
        "PARK" or "GREENERY" or "Greenery"                        => "PARK",
        "MISC." or "MISC" or "Misc." or "MISS" or "MISC "        => "MISC",
        "TEMPLE" or "Temple"                                      => "TEMPLE",
        "DEVISTHAN" or "devisthan"                               => "DEVISTHAN",
        "WATER BODIES" or "WATER_BODIES"                          => "WATER",
        "BEAUTIFICATION"                                          => "BEAUTIFY",
        "TREE"                                                    => "MISC",
        "SASCI"                                                   => "MISC",
        _                                                         => "MISC",
    };

    // ── Fund source normalisation ─────────────────────────────────
    private static string? NormFs(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        return s.Trim().ToUpperInvariant() switch
        {
            "BDA"                    => "BDA",
            "LOAN"                   => "LOAN",
            "GRANT"                  => "GRANT",
            "DEPOSIT" or "DEPOSITE"  => "DEPOSIT",
            "SASCI"                  => "SASCI",
            "TOURISM"                => "BDA",   // treat as BDA allocation
            "NCRPB"                  => "LOAN",
            _                        => null,
        };
    }

    // ── Date parser: handles dd/MM/yyyy, M/d/yyyy, dd.MM.yyyy, dd-MM-yyyy ──
    private static DateOnly? ParseDate(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        raw = raw.Trim().Replace(" ", "");
        char sep = raw.Contains('.') ? '.' : raw.Contains('-') ? '-' : '/';
        var parts = raw.Split(sep);
        if (parts.Length != 3) return null;
        if (!int.TryParse(parts[0], out int a)) return null;
        if (!int.TryParse(parts[1], out int b)) return null;
        if (!int.TryParse(parts[2], out int y)) return null;
        if (y < 100) y += 2000;
        // disambiguate: if a > 12 → a=day, b=month; if b > 12 → a=month, b=day
        int day, month;
        if (a > 12)      { day = a; month = b; }
        else if (b > 12) { day = b; month = a; }
        else             { day = a; month = b; }   // default DD/MM (Indian)
        try { return new DateOnly(y, month, day); }
        catch { return null; }
    }

    // ── Decimal parser: strips %, commas, non-numeric garbage ────
    private static decimal? ParsePct(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        s = s.Trim().TrimEnd('%').Trim();
        if (decimal.TryParse(s, out var v) && v >= 0 && v <= 100) return v;
        return null;
    }

    // ── JEN normalisation table ───────────────────────────────────
    private static readonly Dictionary<string, string> JenUserMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["ANOOP SINGH"]          = "anoop.singh",
            ["NAVEEN GOYAL"]         = "naveen.goyal",
            ["Naveen Goyal"]         = "naveen.goyal",
            ["SANJAY MEENA"]         = "sanjay.meena",
            ["Sanjay Meena"]         = "sanjay.meena",
            ["NARESH"]               = "naresh.singh",
            ["NARESH SINGH"]         = "naresh.singh",
            ["Naresh Singh"]         = "naresh.singh",
            ["Naresh singh"]         = "naresh.singh",
            ["NARESH KUMAR"]         = "naresh.singh",
            ["Naresh singh"]         = "naresh.singh",
            ["KAVITA BHATI"]         = "kavita.bhati",
            ["Kavita Bhati"]         = "kavita.bhati",
            ["SANTOSH"]              = "santosh.kumari",
            ["SANTOSH KUMARI"]       = "santosh.kumari",
            ["SANTOSH KUMAR"]        = "santosh.kumari",
            ["Santosh Kumari"]       = "santosh.kumari",
            ["LAV KUSH YADAV"]       = "lavkush.yadav",
            ["ASHISH KUMAR MAURYA"]  = "ashish.maurya",
            ["ASHISH KUMAR SINGH"]   = "ashish.singh",
            ["ASHISH SINGH"]         = "ashish.singh",
            ["Ashish Singh"]         = "ashish.singh",
            ["ASHISHSINGH"]          = "ashish.singh",
            ["DHARMENDRA"]           = "dharmendra.kumar",
            ["DHARMENDRA KUMAR"]     = "dharmendra.kumar",
            ["Anoop Singh"]          = "anoop.singh",
            ["NARESH SINGH"]         = "naresh.singh",
        };

    private static readonly Dictionary<string, string> JenDisplayMap = new()
    {
        ["anoop.singh"]      = "Anoop Singh",
        ["naveen.goyal"]     = "Naveen Goyal",
        ["sanjay.meena"]     = "Sanjay Meena",
        ["naresh.singh"]     = "Naresh Singh",
        ["kavita.bhati"]     = "Kavita Bhati",
        ["santosh.kumari"]   = "Santosh Kumari",
        ["lavkush.yadav"]    = "Lav Kush Yadav",
        ["ashish.maurya"]    = "Ashish Kumar Maurya",
        ["ashish.singh"]     = "Ashish Kumar Singh",
        ["dharmendra.kumar"] = "Dharmendra Kumar",
    };

    // ═══════════════════════════════════════════════════════════════
    //  Main entry point
    // ═══════════════════════════════════════════════════════════════
    public static async Task SeedAsync(IDbContextFactory<BdaDbContext> factory)
    {
        await using var db = await factory.CreateDbContextAsync();
        if (await db.Works.AnyAsync()) return;  // already seeded

        // ── 1. Ensure missing categories ──────────────────────────
        var newCats = new[] {
            ("TEMPLE",   "Temple works"),
            ("DEVISTHAN","Devisthan works"),
            ("WATER",    "Water body works"),
            ("BEAUTIFY", "Beautification works"),
        };
        foreach (var (code, name) in newCats)
        {
            if (!await db.WorkCategories.AnyAsync(c => c.CategoryCode == code))
                db.WorkCategories.Add(new WorkCategory { CategoryCode = code, CategoryName = name });
        }

        // ── 2. Ensure SASCI fund source ───────────────────────────
        if (!await db.FundSources.AnyAsync(f => f.SourceCode == "SASCI"))
            db.FundSources.Add(new FundSource { SourceCode = "SASCI", SourceName = "SASCI Fund" });

        await db.SaveChangesAsync();

        var catMap = await db.WorkCategories.ToDictionaryAsync(c => c.CategoryCode, c => c.CategoryId);
        var fsMap  = await db.FundSources.ToDictionaryAsync(f  => f.SourceCode,     f => f.FundSourceId);

        // ── 3. Create JEN engineers ───────────────────────────────
        var pwd = BCrypt.Net.BCrypt.HashPassword("Jen@1234");
        var jenIdMap = new Dictionary<string, int>();

        foreach (var (uname, dname) in JenDisplayMap)
        {
            var eng = await db.Engineers.FirstOrDefaultAsync(e => e.Username == uname);
            if (eng == null)
            {
                eng = new Engineer
                {
                    Name = dname, Username = uname,
                    PasswordHash = pwd,
                    Role = EngineerRole.JEN, IsActive = true
                };
                db.Engineers.Add(eng);
                await db.SaveChangesAsync();
            }
            jenIdMap[uname] = eng.EngineerId;
        }

        // ── Lookup helpers ────────────────────────────────────────
        var usedCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        int? JenId(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            if (!JenUserMap.TryGetValue(raw.Trim(), out var u)) return null;
            return jenIdMap.TryGetValue(u, out var id) ? id : null;
        }

        int? CatId(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            var code = NormCat(raw);
            return catMap.TryGetValue(code, out var id) ? id : null;
        }

        int? FsId(string? raw)
        {
            var code = NormFs(raw);
            if (code == null) return null;
            return fsMap.TryGetValue(code, out var id) ? id : null;
        }

        string? UniqueCode(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            var t = raw.Trim();
            if (usedCodes.Add(t)) return t;
            return null;   // duplicate — leave null
        }

        bool IsCm(string? s) =>
            string.Equals(s?.Trim(), "YES", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(s?.Trim(), "yes", StringComparison.OrdinalIgnoreCase);

        bool IsScheme(string? s) =>
            string.Equals(s?.Trim(), "S", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(s?.Trim(), "SCHEME", StringComparison.OrdinalIgnoreCase);

        Work Make(W w) => new()
        {
            WorkCode               = UniqueCode(w.No),
            WorkName               = w.Name.Trim(),
            AssignedJenId          = JenId(w.Jen),
            CategoryId             = CatId(w.Cat),
            FundSourceId           = FsId(w.Fund),
            SanctionedAmount       = w.San,
            AgreementAmount        = w.Agr,
            Expenditure            = w.Exp,
            StartDate              = ParseDate(w.St),
            ExpectedCompletion     = ParseDate(w.En),
            ProgressPercent        = w.Ph,
            FinancialProgressPercent = w.Fin,
            PaymentStatus          = w.PaySt,
            Status                 = w.Status,
            IsAnnualContract       = w.Annual,
            IsScheme               = IsScheme(w.Sc),
            IsCmBudget             = IsCm(w.Cm),
            Department             = string.IsNullOrWhiteSpace(w.Dept) ? null : w.Dept.Trim(),
            ContractorName         = string.IsNullOrWhiteSpace(w.Firm) ? null : w.Firm.Trim(),
            ContractorMobile       = string.IsNullOrWhiteSpace(w.Mob)  ? null : w.Mob.Trim(),
        };

        // ═══════════════════════════════════════════════════════════
        //  IN-PROGRESS WORKS  (57 rows, status = ONGOING)
        // ═══════════════════════════════════════════════════════════
        var inProgress = new W[]
        {
            new("118/2024-25","रैडक्रॉस सर्किल से गोवर्धन गेट तक सडक चौडाईकरण मय डिवाईडर निर्माण एवं सुजान गंगा नहर के चारो तरफ सडक निर्माण कार्य।","ANOOP SINGH",150,134.62m,"07/02/2025","30/04/2026",80,40,"NS","TOURISM","DEPOSIT","YES","ROAD","M/S PUSHPENDRA CHOUDHARY","9460674160"),
            new("145/2024-25","न्यास क्षेत्र में विभिन्न स्थानों पर आवष्यकतानुसार बाउण्ड्री वॉल मय रेलिंग का निर्माण कार्य। (वार्षिक)","ANOOP SINGH",99,80.53m,"06/04/2025","05/06/2026",50,20,"S","BDA","BDA","NO","MISC","M/S UTKARSH CONSTRUCTION","8949744595",WorkStatus.ONGOING,true),
            new("121/2024-25","सूरजपोल चौराहा से मडरपुर चौराहा तक सडक डामरीकरण द्वारा सुदृढीकरण कार्य।","NAVEEN GOYAL",400.94m,339.87m,"02/07/2025","06/02/2026",75,null,"NS","BDA","LOAN","YES","ROAD","M/S PUSHPENDRA CHOUDHARY","9460674160"),
            new("136/2025-26","कृषि उपज मण्डी गेट से आर.एन.एफ.सी.डी. तक आर.सी.सी. नाला निर्माण कार्य","SANJAY MEENA",657.66m,551.37m,"20.09.2025","19.06.2026",20,3,"NS","BDA","LOAN","YES","DRAIN","M/S MODI CONSTRUCTION",null),
            new("129/2025-26","प्राधिकरण क्षेत्र में आवश्यकतानुसार विभिन्न स्थानों पर मोटर पम्प लगाने हेतु फाउण्डेशन सहित नवीन जी.आई. शीट द्वारा शैड का निर्माण कार्य।","NARESH",21.16m,18.53m,"04/09/2025","03/12/2025",null,null,"NS","BDA","BDA","NO","MISC","M/S HARSH TRADING COMPANY",null),
            new("59/2025-26","प्राधिकरण क्षेत्राधिकार के अंतर्गत किराये पर ट्रेक्टर मय ट्रॉली, पॉकलेन मषीन एवं जेसीबी आपूर्ति का कार्य (वार्षिक)","KAVITA BHATI",35.63m,30.6m,"5/30/2025","5/29/2026",null,null,"NS","BDA","BDA","NO","MISC","M/S TOTA CONSTRUCTION COMPANY","9214149999",WorkStatus.ONGOING,true),
            new("91/2024-25","न्यास की योजना संख्या 13 में जे, के, एल, एन, एम ब्लॉक की 36 मीटर, 30 मीटर एवं 24 मीटर एवं अन्य लिंक गलियां में मिट्टी भराई एवं डब्लयू.बी.एम. सडक निर्माण कार्य।","SANTOSH",1065.76m,789.99m,"12/5/2024","4/28/2026",80,70,"S","BDA","LOAN","YES","ROAD","M/S R.S.G.V. INFRA PVT. LTD.",null),
            new("92/2024-25","न्यास की योजना संख्या 13 में ए, बी, सी, डी, ई, एफ, जी, एच ब्लॉक की 36 मीटर, 30 मीटर एवं 24 मीटर एवं अन्य लिंक गलियां में मिट्टी भराई एवं डब्लयू.बी.एम. सडक निर्माण कार्य।","SANTOSH",1175.3m,843.63m,"12/5/2024","2/28/2026",85,50,"S","BDA","LOAN","YES","ROAD","M/S R.S.G.V. INFRA PVT. LTD.",null),
            new("105/2024-25","Selection of service provider for design, procurement, installation, commissioning, operation and maintenance of public bicycle sharing scheme in Bharatpur (Rajasthan)","LAV KUSH YADAV",99.8m,91.75m,"1/12/2025","12/31/2025",70,null,"NS","BDA","BDA","NO","MISC","M/S RAMANUJ POWER CONTROLS PVT LTD",null),
            new("16/2025-26","न्यास आवासों में मरम्मत एवं पेंटिंग संबंधी कार्य।","LAV KUSH YADAV",14.9m,14.61m,"5/1/2025","30.04.2026",60,30,"S","BDA","BDA","NO","BUILDING","M/S MANOJ KUMAR JINDAL",null),
            new("82/2025-26","प्राधिकरण कार्यालय भरतपुर में 04 कमरों का निर्माण कार्य","ASHISH KUMAR MAURYA",98.98m,82.97m,"6/22/2025","30.06.2026",60,null,"S","BDA","BDA","NO","BUILDING","M/S KAPIL JAIN CONTRACTOR",null),
            new("74/2025-26","जिला कलैक्ट्रेट भरतपुर के सभागार में रेनोवेशन, प्रतिक्षालय का निर्माण एवं अन्य मरम्मत कार्य","ASHISH KUMAR MAURYA",26.36m,23.11m,"16/6/2025","15/10/2025",60,null,"NS","BDA","BDA","NO","BUILDING","M/S KAPIL JAIN CONTRACTOR","9828282941"),
            new("74/2024-39","पुराना प्रशासन भवन में दुकानों की डिसमेंटलिंग, बाउन्ड्रीवॉल एवं सी.सी. सडक निर्माण कार्य।","SANTOSH",29.95m,24.08m,"1/24/2025","6/16/2025",60,null,"NS","BDA","BDA","NO","ROAD","M/S SAINI CONSTRUCTION","8005659552"),
            new("113/2025-26","प्राधिकरण की एसपीजेड योजना में आन्तरिक विद्युतीकरण कार्य","DHARMENDRA",2583.4m,2065.84m,"7/24/2025","7/23/2026",50,null,"NS","BDA","LOAN","YES","ELECTRIC","M/S Bhanwariya Infra Project Pvt. Ltd.",null),
            new("03/2025-26","शीशम तिराहे से हीरादास होते हुए सरसों अनुसंधान केन्द्र तक विद्युत लाईन शिफ्टिंग कार्य।","DHARMENDRA",1582.39m,1010m,"08/17/2025","01/16/2026",92,null,"NS","BDA",null,"YES","ELECTRIC","M/S PATHAK ELECTRICALS",null),
            new(null,"विकास प्राधिकरण अधिशाशी अभियंता तृतीय के क्षेत्राधिकार में WBM सी सी एवं डामर पैच मरम्मत कार्य। (वार्षिक)","SANJAY MEENA",33.83m,27.35m,"26.09.2025","25.09.2026",null,null,"NS","BDA","BDA","NO","ROAD","M/S TEJVEER CONTRACTOR","8077106925",WorkStatus.ONGOING,true),
            new(null,"गिर्राज कैनाल का जीर्णोद्धार का कार्य।","NAVEEN GOYAL",5000m,3764m,"05.10.2025","04.01.2027",10,2,"NS","BDA","BDA","YES","DRAIN",null,null),
            new("150/2025-26","जवाहर नगर तिराहे से कब्रिस्तान डी ब्लॉक जवाहर नगर तक ऑवरहेड विद्युत लाइनों को भूमिगत करने का कार्य।","DHARMENDRA",143m,93.2m,"09.11.2025","08.05.2026",30,null,"NS","BDA","BDA","NO","ELECTRIC","M/S A K ELECTRICALS & DECORATION SERVICE",null),
            new("149/2025-26","सलूजा नर्सिंग होम से कलेक्ट्रेट वॉया बी.एस.एन.एल क्वाटर तक ओवरहैड विद्युत लाइन को भूमिगत करने का कार्य।","DHARMENDRA",182m,120.02m,"09.11.2025","08.05.2026",20,null,"NS","BDA","BDA","NO","ELECTRIC","M/S A K ELECTRICALS & DECORATION SERVICE",null),
            new("151/2025-26","कब्रिस्तान डी ब्लॉक जवाहर नगर से मोहन पब्लिक स्कूल तक ऑवरहेड विद्युत लाइनों को भूमिगत करने का कार्य।","DHARMENDRA",152m,100.1m,"09.11.2025","08.05.2026",20,null,"NS","BDA","BDA","NO","ELECTRIC","M/S A K ELECTRICALS & DECORATION SERVICE",null),
            new("155/2025-26","रेड क्रास सर्किल से रेल्वे स्टेशन तक ओवरहैड विद्युत लाईन को भूमिगत करने का कार्य","DHARMENDRA",239m,158.45m,"28.11.2025","27.05.2026",20,null,"NS","BDA","BDA","YES","ELECTRIC",null,null),
            new(null,"मंडी परिसर में ड्रेन का निर्माण कार्य।","ASHISH KUMAR SINGH",307m,260.67m,"08.12.2025","07.06.2026",60,35,"SCHEME","BDA","LOAN","NO","DRAIN","M/S Jain Builder",null),
            new(null,"किसान भवन से कुम्हेर गेट सी.एस.सी.डी. तक ड्रेन का निर्माण कार्य।","ASHISH KUMAR SINGH",497m,421.81m,"08.12.2025","07.06.2026",18,6,"NS","BDA","LOAN","NO","DRAIN","Modi Construction",null),
            new(null,"मथुरा बाईपास से गोल-मोल बगीची तक ड्रेन का निर्माण कार्य।","ASHISH KUMAR SINGH",1177m,1003.11m,"08.12.2025","07.08.2026",10,null,"NS","BDA","LOAN","NO","DRAIN","M/S Jain Builder",null),
            new(null,"मथुरा बाईपास से कुम्हेर गेट तक ड्रेन का निर्माण कार्य।","ASHISH KUMAR SINGH",1675m,1413.57m,"08.12.2025","07.12.2026",2,null,"NS","BDA","LOAN","NO","DRAIN","M/S Dinesh Construction",null),
            new(null,"मथुरा बाईपास गोलपुरा चौराहा से अखड्ड तिराहा होते हुए मण्डी गेट तक ड्रेन का निर्माण कार्य।","ASHISH KUMAR SINGH",1832m,1446.33m,"14.12.2025","13.03.2027",15,null,"NS","BDA","LOAN","NO","DRAIN","M/S Jain Builder",null),
            new(null,"मथुरा बाईपास से होटल रॉयल हेरिटेज होटल से सी.एफ.सी.डी. तक ड्रेन का निर्माण कार्य।","ASHISH KUMAR SINGH",2168m,1705.36m,"16.12.2025","15.06.2027",3,2,null,null,null,null,"DRAIN","M/S Jain Builder",null),
            new(null,"गोलमोल बगीची से तोप सर्किल होते हुए सीएफसीडी तक ड्रैन निर्माण कार्य।","ASHISH KUMAR SINGH",1057.9m,845m,"18-03-2026","17-03-2027",2,null,"NS","BDA","BDA","no","DRAIN","m/s Shyama Construction",null),
            new(null,"भरतपुर में गंगा मंदिर एवं लक्ष्मण मंदिर के अग्रभाग का संरक्षण एवं सौन्दर्यीकरण का कार्य","LAV KUSH YADAV",490.92m,371.89m,"13/12/2025","12/06/2026",25,null,"NS","BDA","GRANT","YES","DEVISTHAN","M/S Hari Prasad Sharma",null),
            new("171/2025-26","सलूजा नर्सिंग होम से बी.नारायण गेट तक ऑवरहैड विद्युत लाईन को भूमिगत करने का कार्य।","ASHISH KUMAR SINGH",248.60m,171.38m,"25/12/2025","24/06/2026",null,null,"NS","BDA","BDA","NO","ELECTRIC",null,null),
            new("172/2025-26","कन्नी गुर्जर से गोविन्द रिसोर्ट सर्किल तक ऑवरहैड विद्युत लाईन को भूमिगत करने का कार्य।","DHARMENDRA",268m,191.15m,"25/12/2025","24/06/2026",40,null,"NS","BDA","BDA","NO","ELECTRIC",null,null),
            new("178/2025-26","स्टेशन रोड बजरिया तिराहे से डीग रोड तिराहा वाया जिन्दल हाॅस्पीटल आॅवरहैड विद्युत लाईन को भूमिगत करने का कार्य।","DHARMENDRA",883.71m,718.18m,"16/1/2026","15/09/2026",50,null,"NS","BDA","BDA","NO","ELECTRIC",null,null),
            new("179/2025-26","सुजान गंगा नहर के चारो तरफ आॅवहैड विद्युत लाईन को भूमिगत करने का कार्य।","DHARMENDRA",975.95m,837.71m,"15/1/2026","14/09/2026",10,null,"NS","BDA","BDA","NO","ELECTRIC",null,null),
            new(null,"रेडक्रास सर्किल से रेल्वे स्टेशन तक रोड बाइन्डिंग का कार्य","NAVEEN GOYAL",700m,524.84m,"03.02.2026","02.10.2026",null,null,null,null,null,null,"ROAD",null,null),
            new(null,"Construction of road widening renovation RCC Nallah and Road Divider work of Circular Road from RBM Hospital to Scheme no. 3 Via Chandpole Gate Bharatpur","ANOOP SINGH",2872.16m,2391.45m,"07.02.2026","06.02.2027",3,null,"NS","BDA","BDA","NO","ROAD","M/S MAHESH CHAND GUPTA",null),
            new("188/2025-26","आर.बी.एम. हाॅस्पीटल से रेडक्राॅस सर्किल तक आॅवरहैड विद्युत लाईनों को भूमिगत करने का कार्य।","DHARMENDRA",362m,307.2m,"3/2/2026","02/08/2026",null,null,"NS","BDA","BDA","NO","ELECTRIC",null,null),
            new("181/2025-26","Panch Gaurav Promotion Art work to be installed in multipurpose Circle Bharatpur","NARESH",65.78m,55.3m,"22/01/2026","21/05/2026",80,null,"NS","BDA","DEPOSIT","NO","MISC","M/S YAWEDO",null),
            new(null,"सुजान गंगा नहर में म्यूजीकल फाउंटेन लगाने का कार्य।","DHARMENDRA",276m,184.34m,"03.02.2026","02.05.2026",30,null,"NS","BDA","DEPOSIT","YES",null,null,null),
            new(null,"श्री गंगा मंदिर का जीर्णोद्धार एवं मरम्मत कार्य।","LAV KUSH YADAV",957m,957m,"18.03.2026","18.03.2027",3,null,"NS","BDA","DEPOSIT","YES","TEMPLE","M/S Sri Sai",null),
            new(null,"Conservation & development of Heritage Building Near Kishori Mahal at Bharatpur","LAV KUSH YADAV",572m,426m,"19.03.2026","19.06.2027",3,null,"NS","BDA","DEPOSIT","NO","BUILDING","m/s khandelwal",null),
            new(null,"भगत सिंह सर्किल से शीशम तिराहा तक ओवरहैड विद्युत लाइन को भूमिगत करने का कार्य।","DHARMENDRA",327m,275.16m,"16.03.2026","15.09.2026",null,null,null,null,null,null,"ELECTRIC",null,null),
            new(null,"सारस चौराहे से बिजलीघर चौराहा तक विद्युत लाइन को भूमिगत करने का कार्य।","DHARMENDRA",370m,312.77m,"27.03.2026","26.09.2026",null,null,null,null,null,null,"ELECTRIC",null,null),
            new(null,"आर.बी.एम. हॉस्पीटल से रीको रोड तक ओवरहैड विद्युत लाइन को भूमिगत करने का कार्य।","DHARMENDRA",593m,524.1m,"16.03.2026","15.09.2026",null,null,null,null,null,null,"ELECTRIC",null,null),
            new(null,"मानसिंह सर्किल से होटल द बाग तक ओवरहैड विद्युत लाइन को भूमिगत करने का कार्य।","DHARMENDRA",684m,574.04m,"20.03.2026","19.09.2026",null,null,null,null,null,null,"ELECTRIC",null,null),
            new(null,"हीरादास बस स्टैण्ड से वाया सुभाष नगर गौशाला से मथुरा बाईपास तक ओवरहैड विद्युत लाइन को भूमिगत करने का कार्य।","DHARMENDRA",698m,590.58m,"16.03.2026","15.09.2026",null,null,null,null,null,null,"ELECTRIC",null,null),
            new(null,"कुम्हेर गेट सर्किल से कन्जोली वाया एम.ई.एस. तक ऑवरहेड विद्युत लाइन को भूमिगत करने का कार्य।","DHARMENDRA",723m,611.46m,"27.03.2026","26.09.2026",null,null,null,null,null,null,"ELECTRIC",null,null),
            new(null,"रेडक्रॉस सर्किल से चाँदपोल गेट तक ऑवरहेड विद्युत लाइन को भूमिगत करने का कार्य।","DHARMENDRA",503.32m,444.09m,"16.03.2026","15.09.2026",null,null,null,null,null,null,"ELECTRIC",null,null),
            new(null,"Construction widening, beautification of Road from Sheesham Tiraha to Kali Bagichi to Heeradas Circle, Chamad Mata Mandir to Sarso Anushandhan Kendra with drainage for storm water at Bharatpur","ANOOP SINGH",6715m,6715m,"02-03-2026","01.09.2027",3,null,"NS","BDA","BDA","no","ROAD","M/s R P Sharma",null),
            new(null,"Upliftment and beautification of junctions namely Kali ki bagichi, traffic chouraha, Seesham Tiraha entry plaza, Circuit house junction and Redcross junction at Bharatpur City","NARESH",4298.09m,4448.22m,"28-03-2026","27-03-2028",null,null,"NS","BDA","BDA",null,"ROAD","M/S Niraniya Construction Company",null),
            new(null,"भरतपुर शहर के मध्य स्थित ऐतिहासिक श्री लक्ष्मण मंदिर का जीर्णोद्धार एवं मरम्मत कार्य।","LAV KUSH YADAV",175m,123.22m,"10-04-2026","10-10-2026",1,null,"NS","BDA","BDA","yes","TEMPLE","M/S Bhoodev Solanki",null),
            new(null,"श्री बांके बिहारी मंदिर का जीर्णोद्धार एवं मरम्मत कार्य।","LAV KUSH YADAV",427m,333.71m,"30-03-2026","29-06-2027",2,null,"NS","BDA","BDA","yes","TEMPLE","M/S Sri SAI",null),
        };

        foreach (var w in inProgress) db.Works.Add(Make(w));
        await db.SaveChangesAsync();

        // ═══════════════════════════════════════════════════════════
        //  ANNUAL CONTRACT WORKS
        // ═══════════════════════════════════════════════════════════
        WorkStatus AnnualStatus(string? s)
        {
            if (s == null) return WorkStatus.ONGOING;
            var t = s.Trim().ToUpperInvariant();
            if (t is "COMPLETED" or "COMPLETE" or "कार्य पूर्ण") return WorkStatus.COMPLETED;
            if (t is "HOLD") return WorkStatus.STALLED;
            return WorkStatus.ONGOING;
        }

        var annual = new (W Work, string? RawStatus)[]
        {
            (new("219/2023-24","न्यास की सैक्टर 03 एवं मुखर्जी नगर योजना में नाली/नाला एवं क्रॉस निर्माण तथा मरम्मत कार्य। (वार्षिक अनुबंध)","SANJAY MEENA",14.84m,11.08m,"3/15/2024","3/14/2025",null,null,"S","BDA","BDA","NO","DRAIN","M/S A.K. INDUSTRIES",null,WorkStatus.ONGOING,true),"Completed"),
            (new("11/2024-25","न्यास की योजना सैक्टर 03 में स्थित गौरव बेटी पार्क की देख-रेख एवं संधारण का कार्य। (वार्षिक अनुबंध)","SANJAY MEENA",14.97m,12.27m,"7/15/2024","10/14/2025",null,null,"S","BDA","BDA","NO","PARK","M/S ANIL KUMAR","6350315080",WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("205/2023-24","गैर योजना क्षेत्र डीग-कुम्हेर रोड से मथुरा बाईपास के मध्य क्षेत्र में विभिन्न स्थानों पर आवष्यकतानुसार नाली क्रॉस एवं मरम्मत कार्य। (वार्षिक अनुबंध)","SANJAY MEENA",9.8m,7.26m,"3/15/2024","3/14/2025",null,null,"NS","BDA","BDA","NO","DRAIN","M/S LOHAGARH GREEN NURSERY & WATER SUPPLIERS","9468583843",WorkStatus.ONGOING,true),"Completed"),
            (new("31/2024-25","न्यास क्षेत्र में क्षतिग्रस्त सड़कों पर डामर पैच मरम्मत कार्य। (वार्षिक अनुबंध)","ANOOP SINGH",49.6m,40.35m,"8/15/2024","8/14/2025",null,null,"NS","BDA","BDA","NO","ROAD","M/S VIJAYBHAN SINGH",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("36/2024-25","न्यास क्षेत्र में विभिन्न स्थानों पर आवश्यकतानुसार वायर फेन्सिग लगाने का कार्य। (वार्षिक अनुबंध)","ANOOP SINGH",48m,43.24m,"8/17/2024","8/16/2025",null,null,"S","BDA","BDA","NO","MISC","M/S AVINASH KUMAR GOYAL","9414268338",WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("124/2025-26","हरियालो राजस्थान अभियान के अंतर्गत प्राधिकरण क्षेत्राधिकार में विभिन्न स्थानों पर आवश्यकतानुसार पेड-पौधे लगाने एवं संधारण का कार्य। (द्विवार्षिक)","ANOOP SINGH",98.45m,94.21m,"21.08.2025","20.08.2027",null,null,"S","BDA","BDA","NO","PARK","M/S LOHAGARH GREEN NURSERY & WATER SUPPLIERS",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("41/2024-25","न्यास क्षेत्र में विभिन्न स्थानों पर आवश्यकतानुसार बाउन्ड्री वॉल का निर्माण कार्य। (वार्षिक)","ANOOP SINGH",49.93m,38.71m,"8/30/2024","8/29/2025",null,null,"S","BDA","BDA","NO","BUILDING","M/S UTKARSH CONSTRUCTION",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("83/2025-26","भरतपुर विकास प्राधिकरण क्षेत्र में विभिन्न स्थानों पर आवष्यकतानुसार नाला, नाली एवं क्रॉस मरम्मत तथा निर्माण कार्य। (वार्षिक)","ANOOP SINGH",97.89m,73.7m,"6/26/2025","6/25/2026",null,null,"NS","BDA","BDA","NO","DRAIN","M/S SHYAM SUNDER SHARMA",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("116/2024-25","प्राधिकरण क्षेत्र में विभिन्न स्थानों पर आवश्यकतानुसार नाली निर्माण कार्य (वार्षिक)","ANOOP SINGH",25m,20.81m,"1/7/2025","1/6/2026",25,null,"NS","BDA","BDA","NO","DRAIN","M/S DOLLY CONSTRUCTION",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("32/2024-25","न्यास क्षेत्र में विभिन्न स्थानों पर नाला/नाली, क्रॉस जाल एवं सी.सी. पैच मरम्मत कार्य। (वार्षिक अनुबंध)","ASHISH KUMAR MAURYA",70.73m,57.16m,"8/15/2024","8/14/2025",null,null,"NS","BDA","BDA","NO","DRAIN","M/S BAJRANG CONSTRUCTION",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("50/2025-26","अछनेरा रोड से लुधावाई टोल प्लाजा के मध्य प्राधिकरण क्षेत्राधिकार के अन्तर्गत विभिन्न स्थानों पर स्पीड ब्रेकर, केटाई ब्लिंकर्स, थर्मो प्लास्टिक पेंट एवं साइन बोर्ड लगाने का कार्य। (वार्षिक)","ASHISH KUMAR MAURYA",25.04m,17.6m,"6/1/2025","5/31/2026",null,null,"NS","BDA","BDA","NO","MISC","M/S PC CONSTRUCTION",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("41/2025-26","अछनेरा रोड से लुधावाई टोल प्लाजा के मध्य प्राधिकरण क्षेत्राधिकार के अन्तर्गत अल्टीनेटर (पम्प सेट) लगाने का कार्य। (वार्षिक अनुबंध)","NARESH SINGH",28.85m,22.43m,"6/13/2025","6/12/2026",null,null,"NS","BDA","BDA","NO","MISC","M/S NARESH CONSTRUCTION",null,WorkStatus.ONGOING,true),"COMPLETE"),
            (new("96/2025-26","प्राधिकरण के क्षेत्र में अधिशाषी अभियंता द्वितीय के क्षेत्राधिकार के अन्तर्गत आवश्यकतानुसार नाला/नाली निर्माण/मरम्मत का कार्य। (वार्षिक)","NARESH SINGH",44.09m,33.2m,"7/5/2025","7/4/2026",null,null,"NS","BDA","BDA","NO","DRAIN","M/S P.C CONSTRUCTION",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("134/2025-26","प्राधिकरण क्षेत्राधिकार में अछनेरा रोड से लुधावई टोल प्लाजा के मध्य क्षेत्र में डामर एवं सी.सी. पैच मरम्मत कार्य। (वार्षिक)","NARESH SINGH",49.8m,42.91m,"13.09.2025","12.09.2026",null,null,"NS","BDA","BDA","NO","ROAD","M/S P C CONSTRUCTION",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("110/2025-26","विकास प्राधिकरण क्षेत्र में अधिशाषी अभियंता द्वितीय के क्षेत्राधिकार के अन्तर्गत अछनेरा रोड से लुधावई टोल प्लाजा के मध्य क्षेत्र में वाउन्ड्रीवॉल, फैन्सिंग एवं सीसी सडक निर्माण कार्य (वार्षिक)","NARESH SINGH",58.4m,45.34m,"7/20/2025","7/19/2026",null,null,"NS","BDA","BDA","NO","MISC","M/S PC CONSTRUCTION",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("77/2025-26","प्राधिकरण क्षेत्र में अधिशाषी अभियंता द्वितीय के क्षेत्राधिकार के अंतर्गत आवश्यकतानुसार विभिन्न आवश्यक निर्माण कार्य (वार्षिक)","NARESH SINGH",41.02m,33.41m,"6/19/2025","6/15/2026",null,null,"NS","BDA","BDA","NO","MISC","M/S KRISHNA CONSTRUCTION",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("122/2025-26","अछनेरा रोड से लुधावई टोल प्लाजा के मध्य प्राधिकरण क्षेत्र में किराये पर जे.सी.बी. आपूर्ति का कार्य (वार्षिक)","NARESH SINGH",9.88m,9.24m,"10.08.2025","09.08.2026",null,null,"NS","BDA","BDA","NO","MISC","M/S DEEPAK DAGUR",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("84/2025-26","भरतपुर विकास प्राधिकरण क्षेत्र में विभिन्न स्थानों पर आवश्यकतानुसार फुटपाथ एवं पाथवे का निर्माण कार्य। (वार्षिक)","ASHISH KUMAR MAURYA",99.95m,80.3m,"6/26/2025","6/25/2026",null,null,"NS","BDA","BDA","NO","ROAD","M/S SHYAM SUNDER SHARMA",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("95/2024-25","आई.जी. ऑफिस के लोन में घास, इंटरलॉकिंग लगाने एवं देखरेख का कार्य।","NARESH SINGH",10.19m,9.15m,"12/14/2024","12/13/2025",null,null,"NS","BDA","BDA","NO","PARK","M/S SOGARWAL CONSTRUCTION",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("114/2025-26","अछनेरा रोड से लुधावई टोल प्लाजा तक प्राधिकरण क्षेत्र में आवश्यकतानुसार पाइप लाईन मरम्मत का कार्य। (वार्षिक)","NARESH SINGH",10.65m,9.66m,"13.07.2025","12.07.2026",null,null,"NS","BDA","BDA","NO","MISC","M/s Krishna ConsTRUCTION",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("50/2024-25","न्यास क्षेत्र में विभिन्न स्थानों पर आवष्यकतानुसार रेलिंग लगाने का कार्य। (वार्षिक)","ASHISH KUMAR MAURYA",99.66m,79.16m,"9/7/2024","9/6/2025",null,null,"NS","BDA","BDA","NO","MISC","M/S BHOLEY SHANKAR GARG","9887409786",WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("96/2024-25","भरतपुर शहर में स्थित जिला क्लब व न्यास के अन्य पार्कों में देख-रेख एवं सधारण कार्य। (वार्षिक)","ASHISH KUMAR MAURYA",5m,3.91m,"12/16/2024","12/15/2025",null,null,"NS","BDA","BDA","NO","PARK","M/S V.P. NURSERY","8290251401",WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("17/2025-26","जिला कलेक्ट्रेट भवन परिसर में आवश्यक निर्माण एवं मरम्मत कार्य। (वार्षिक अनुबंध)","ASHISH KUMAR MAURYA",29.21m,22.87m,"5/1/2025","4/30/2026",null,null,"NS","BDA","BDA","NO","BUILDING","M/S BHOLEY SHANKAR GARG",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("63/2025-26","प्राधिकरण क्षेत्राधिकार के अंतर्गत एच टी एवं एल टी विद्युत लाईन तथा स्ट्रीट लाईट पोल षिफ्टिंग का कार्य (वार्षिक)","DHARMENDRA",430m,405.12m,"6/4/2025","03/06/2026",null,null,"NS","BDA","BDA","NO","ELECTRIC","M/S RAWAT ELECTRICALS",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("27/2024-25","न्यास ऑडिटोरियम में विद्युत व्यवस्था रखरखाव, मरम्मत एवं संचालन का कार्य। (वार्षिक अनुबंध)","DHARMENDRA",14.5m,11.79m,"8/11/2024","8/10/2025",null,null,"S","BDA","BDA","NO","ELECTRIC","M/S PATHAK ELECTRICALS",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("73/2024-25","वित्तीय वर्ष 2024-25 हेतु न्यास कार्यालय एवं न्यास आवासों में विद्युत रख-रखाव, मरम्मत एवं संचालन का कार्य। (वार्षिक अनुबंध)","DHARMENDRA",13.98m,11.25m,"10/4/2024","10/3/2025",null,null,"S","BDA","BDA","NO","ELECTRIC","M/S N&D INFRA",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("23/2024-25","न्यास की विभिन्न योजनाओं में डिमार्केशन का कार्य। (वार्षिक अनुबंध)","LAV KUSH YADAV",14.13m,11.63m,"10/24/2024","10/23/2025",null,null,"S","BDA","BDA","NO","MISC","M/S P C CONSTRUCTION",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("16/2025-26-AC","न्यास आवासों में मरम्मत एवं पेटिंग संबंधी कार्य। (वार्षिक)","ASHISH KUMAR MAURYA",14.96m,14.61m,"5/1/2025","4/30/2026",null,null,"S","BDA","BDA","NO","BUILDING","M/S MANOJ KUMAR JINDAL",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("81/2024-25","वित्तीय वर्ष 2024-25 हेतु वी.पी.एस. पार्क एवं गौरव बेटी पार्कों में विद्युत रख-रखाब, मरम्मत एवं संचालन का कार्य। (वार्षिक)","DHARMENDRA",14.68m,10.52m,"10/27/2024","10/26/2025",null,null,"S","BDA","BDA","NO","ELECTRIC","M/S PATHAK ELECTRICALS",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("26/2025-26","सुजान गंगा नहर में स्थापित फब्बारों की देखभाल, मय मैटेरियल मरम्मत, सुरक्षा व्यवस्था एवं संचालन का कार्य (त्रिवार्षिक अनुबंध)","DHARMENDRA",13.16m,12.97m,"5/2/2025","5/1/2028",null,null,"NS","BDA","BDA","NO","ELECTRIC","M/S N & D INFRA",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("142/2024-25","प्राधिकरण चौराहा, हीरादास चौराहा एवं यातायात तिराहे पर स्थित फब्बारों का संचालन कार्य। (वार्षिक)","DHARMENDRA",5.02m,3.62m,"3/30/2025","3/29/2026",null,null,"NS","BDA","BDA","NO","ELECTRIC","M/S N&D INFRA",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("38/2025-26","विश्वप्रिय शास्त्री पार्क में म्यूजिकल फाउण्टेन मरम्मत एवं मय मैटेरियल संचालन कार्य (त्रिवार्षिक)","DHARMENDRA",13m,11.82m,"17.05.2025","16.05.2028",null,null,"S","BDA","BDA","NO","ELECTRIC","M/S FAMOUS FOUNTAINS",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("123/2025-26","प्राधिकरण क्षेत्राधिकार में योजना तथा गैर-योजना क्षेत्र में पूर्व में स्थापित की गई स्ट्रीट लाईटों की मरम्मत एवं रख-रखाव का कार्य। (वार्षिक)","DHARMENDRA",99.9m,89.72m,"21.08.2025","20.08.2026",null,null,"S","BDA","BDA","NO","ELECTRIC","M/S PATHAK ELECTRICALS",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("60/2025-26","प्राधिकरण क्षेत्राधिकार के अंतर्गत आवष्यकता अनुसार नाला/नाली निर्माण एवं मरम्मत तथा वर्षा जल निकासी हेतु डीजन इंजन लगाने कार्य (वार्षिक)","NAVEEN GOYAL",99.19m,74.91m,"6/12/2025","6/11/2026",null,null,"NS","BDA","BDA","NO","DRAIN","M/S GANPATI CONSTRUCTION",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("104/2025-26","प्राधिकरण क्षेत्र में अधिशाषी अभियंता तृतीय के क्षेत्राधिकार में विभिन्न स्थानों पर आवश्यकतानुसार नाली, क्रास, डब्ल्यूबीएम एवं सीसी सडक निर्माण एवं मरम्मत कार्य (वार्षिक)","NAVEEN GOYAL",96.88m,73.7m,"7/14/2025","7/13/2026",null,null,"NS","BDA","BDA","NO","ROAD","M/s Tejveer Contractor",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("106/2025-26","विकास प्राधिकरण क्षेत्र में अधिशाषी अभियंता तृतीय के क्षेत्राधिकार के अन्तर्गत विभिन्न स्थानों पर आवश्यकतानुसार पाईपलाईन डालने, जोडने एवं लीकेज मरम्मत कार्य (वार्षिक)","NAVEEN GOYAL",81m,62.46m,"7/18/2025","7/17/2026",null,null,"NS","BDA","BDA","NO","MISC","M/s Krishna ConsTRUCTION",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("105/2025-26","विकास प्राधिकरण क्षेत्र में विभिन्न स्थानों पर आवश्यकतानुसार सडक के दोनों तरफ पेड शिफ्टिंग एवं रखरखाव कार्य (वार्षिक)","KAVITA BHATI",4.03m,49.78m,"7/18/2025","7/17/2026",null,null,"NS","BDA","BDA","NO","MISC","M/s Green Earth Enterprises",null,WorkStatus.STALLED,true),"HOLD"),
            (new(null,"भरतपुर शहर में प्राधिकरण क्षेत्र के अंतर्गत विभिन्न राजकीय आयोजनों एवं राष्ट्रीय त्योहारों पर अस्थायी रोशनी व्यवस्था एवं सजावट कार्य। (वार्षिक)","DHARMENDRA KUMAR",93.14m,79.34m,"17/10/2025","16/10/2026",null,null,"NS","BDA","BDA","NO","ELECTRIC","M/S ANJU ELECTRIC & SOUND SERVICES",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("19/2025-26","अछनेरा रोड से एस.पी.जेड. योजना तक समस्त क्षेत्र में एल.टी. एवं एच.टी. विद्युत लाइन षिफ्टिंग का कार्य।","DHARMENDRA",58.3m,47.05m,"4/30/2025","4/29/2026",null,null,"NS","BDA","BDA","NO","ELECTRIC","M/S PATHAK ELECTRICALS",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new(null,"प्राधिकरण ऑडिटोरियम में विद्युत व्यवस्था रख-रखाव, मरम्मत एवं संचालन कार्य। (वार्षिक)","DHARMENDRA KUMAR",21.76m,17.76m,"17/10/2025","16/10/2026",null,null,"S","BDA","BDA","NO","ELECTRIC","M/S N & D INFRA",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new(null,"प्राधिकरण कार्यालय भवन परिसर एवं आवासों में विद्युत व्यवस्था रख-रखाव, मरम्मत एवं संचालन कार्य। (वार्षिक)","DHARMENDRA KUMAR",33.84m,28.60m,"17/10/2025","16/10/2026",null,null,"S","BDA","BDA","NO","ELECTRIC","M/S N & D INFRA",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new(null,"वी.पी.एस. पार्क एवं गौरव बेटी पार्क में विद्युत व्यवस्था रख-रखाव, मरम्मत एवं संचालन कार्य। (वार्षिक)","DHARMENDRA KUMAR",35.49m,28.93m,"17/10/2025","16/10/2026",null,null,"S","BDA","BDA","NO","ELECTRIC","M/S RADHIKA TRADERS",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new(null,"अछनेरा रोड से लुधावई टोल प्लाजा तक प्राधिकरण क्षेत्र में आवश्यकतानुसार ट्रैक्टर ट्रॉली एवं पोकलेन की सप्लाई का कार्य। (वार्षिक)","ASHISH KUMAR SINGH",25.83m,null,null,null,null,null,"NS","BDA","BDA","NO","MISC","M/S TOTA CONSTRUCTION COMPANY",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("135/2025-26","पंक्षी का नगला राजकीय विद्यालय के निकट तथा अन्य स्थानों पर वर्षा जल निकासी हेतु मोटर मडपम्प संचालन एवं मरम्मत कार्य (वार्षिक)","NARESH",9.93m,9.41m,"19.09.2025","18.03.2026",null,null,"NS","BDA","BDA","NO","MISC","M/S DANDOTIA CONSTRUCTION",null,WorkStatus.ONGOING,true),"IN PROGRESS"),
            (new("95/2025-26","वी.पी.एस. पार्क भरतपुर में पौधा रोपण एवं उनके रख रखाव का कार्य। (वार्षिक)","NARESH",14.2m,7.88m,"7/4/2025","7/3/2026",10,null,"S","BDA","BDA","NO","MISC","M/S V P NURSERY","8290251401",WorkStatus.ONGOING,true),"IN PROGRESS"),
        };

        foreach (var (w, rawSt) in annual)
        {
            var work = Make(w);
            work.Status = AnnualStatus(rawSt);
            work.IsAnnualContract = true;
            db.Works.Add(work);
        }
        await db.SaveChangesAsync();

        // ═══════════════════════════════════════════════════════════
        //  COMPLETED WORKS
        //  Status col: "कार्य पूर्ण" / "100%" → COMPLETED
        //              "IN PROGRESS" / % → ONGOING
        //              "Work Withdrawal" → CANCELLED
        // ═══════════════════════════════════════════════════════════
        WorkStatus CompStatus(string? s, decimal? phys)
        {
            if (s == null) return WorkStatus.COMPLETED;
            var t = s.Trim();
            if (t is "IN PROGRESS" or "in progress") return WorkStatus.ONGOING;
            if (t.Equals("Work Withdrawal", StringComparison.OrdinalIgnoreCase)) return WorkStatus.CANCELLED;
            if (t == "100" || t == "100%") return WorkStatus.COMPLETED;
            if (phys.HasValue && phys < 100) return WorkStatus.ONGOING;
            return WorkStatus.COMPLETED;
        }

        var completed = new (string? No, string Name, string Jen, decimal? San, decimal? Agr,
            string? St, string? En, string? StatusRaw, decimal? Ph,
            string? Sc, string? Dept, string? Fund, string? Cm,
            string? Cat, string? Firm, string? Mob, decimal? Exp)[]
        {
            ("106/2024-25","गोविन्द निवास कॉलोनी में सी.सी. सड़क निर्माण कार्य।","ASHISH KUMAR MAURYA",13.85m,10.6m,"1/24/2025","3/23/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","ROAD","M/S SAINI CONSTRUCTION",null,11.41m),
            ("75/2024-51","न्यास की एस.पी.जेड. योजना में सी, एफ, जी एवं एच ब्लॉक में डब्ल्यू.बी.एम. सडक का निर्माण कार्य।","NARESH SINGH",105.74m,66.79m,"9/7/2024","3/6/2025","कार्य पूर्ण",null,"S","BDA","BDA","YES","ROAD","M/S PUSHPENDRA CHOUDHARY",null,52.34m),
            ("102/2024-25","न्यास कार्यालय में सेन्ट्रल ए.सी. सचिव महोदय एवं अध्यक्ष कक्ष में लगाने का कार्य।","DHARMENDRA",4.93m,4.65m,"1/2/2025","2/1/2025","कार्य पूर्ण",null,"S","BDA","BDA","NO","ELECTRIC","M/S PATHAK ELECTRICALS",null,null),
            ("114/2024-25","सुजान गंगा नहर के वोटिंग क्षेत्र से पार्क तक लाइटिंग का कार्य।","DHARMENDRA",11.62m,7.8m,"2/2/2025","4/1/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","ELECTRIC","M/S PATHAK ELECTRICALS",null,null),
            ("111/2024-25","विकास प्राधिकरण के कार्यालय भवन के प्रथम तल पर विभिन्न कक्षों में एल्यूमिनियम पार्टीशन लगाने का कार्य।","LAV KUSH YADAV",4.97m,4.5m,"1/30/2025","2/28/2025","कार्य पूर्ण",null,"S","BDA","BDA","NO","BUILDING","M/S LALITA PRASAD",null,null),
            ("137/2024-25","विकास प्राधिकरण कार्यालय एवं अध्यक्ष महोदय के कक्ष में रेनोवेषन एवं अन्य मरम्मत कार्य।","LAV KUSH YADAV",3.67m,3.32m,"3/15/2025","4/14/2025","कार्य पूर्ण",null,"S","BDA","BDA","NO","BUILDING","M/S KAPIL JAIN CONTRACTOR",null,3.63m),
            ("130/2024-25","सैक्टर 03 योजना आशीर्वाद फ्लेट के पीछे शास्त्रीपुरम कॉलोनी में सी.सी. सडक निर्माण कार्य।","SANJAY MEENA",28.58m,21.61m,"2/24/2025","5/23/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","ROAD","M/S SAINI CONSTRUCTION",null,9.23m),
            ("139/2024-25","एस.पी.जे. कुण्ड के चारों तरफ ट्रैक निर्माण हेतु मिट्टी भर्त का कार्य।","NARESH SINGH",4.99m,4.54m,"3/22/2025","4/21/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","MISC","M/S VIJAYBHAN SINGH",null,null),
            ("55/2024-25","न्यास कार्यालय में सचिव एवं अध्यक्ष के कक्षों में रेनोवेषन एवं अन्य कार्य।","LAV KUSH YADAV",14.6m,14.52m,"9/14/2024","9/13/2024","कार्य पूर्ण",null,"S","BDA","BDA","NO","BUILDING","M/S KAPIL JAIN CONTRACTOR",null,8.94m),
            ("133/2024-25","जिला कलक्टर एवं अध्यक्ष महोदय के कैम्प ऑफिस में तिरंगा झण्डा हेतु ग्रेनाइट का फाउण्डेशन, बाउण्ड्री वॉल निर्माण एवं अन्य मरम्मत कार्य।","ASHISH KUMAR MAURYA",3.71m,3.36m,"3/13/2025","4/13/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","BUILDING","M/S KAPIL JAIN CONTRACTOR",null,3.36m),
            ("218/2023-24","न्यास की योजनाओं में स्थापित विद्युत लाइटों की देखभाल एवं मरम्मत कार्य हेतु फॉल्ट रेक्टिफिकेषन टीम लगाने का कार्य। (वार्षिक अनुबंध)","DHARMENDRA",17.55m,10.51m,"3/15/2024","3/14/2024","कार्य पूर्ण",null,"S","BDA","BDA","NO","ELECTRIC","M/S PATHAK ELECTRICALS",null,16.46m),
            ("28/2024-25","न्यास क्षेत्र में विभिन्न स्थानों पर आवष्यकतानुसार ट्रैक्टर मय ट्रॉली आपूर्ति का कार्य। (वार्षिक अनुबंध)","ANOOP SINGH",9.67m,9.33m,"8/15/2024","8/14/2024","कार्य पूर्ण",null,"NS","BDA","BDA","NO","MISC","M/S LOHAGARH GREEN NURSERY & WATER SUPPLIERS",null,9.31m),
            ("09/2025-26","सर्किट हाउस में जन-सुनवाई हेतु पाथवे पर स्टील रैलिंग लगाने का कार्य।","NAVEEN GOYAL",4.89m,4.68m,"4/26/2025","5/25/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","ROAD","M/S AVINASH KUMAR GOYAL",null,null),
            ("10/2025-26","सर्किट हाउस में जन-सुनवाई हेतु पाथवे पर चैकर्ड टाइल्स एवं गेट लगाने का कार्य।","NAVEEN GOYAL",4.95m,4.56m,"4/24/2025","5/23/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","ROAD","M/S AVINASH KUMAR GOYAL",null,null),
            ("11/2025-26","सर्किट हाउस में इंटरलॉकिंग पाथवे निर्माण एवं अन्य मरम्मत कार्य।","NAVEEN GOYAL",4.99m,4.59m,"4/24/2025","5/23/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","ROAD","M/S AVINASH KUMAR GOYAL",null,null),
            ("84/2024-25","एस.पी.एम. नगर योजना में सेन्ट पॉल स्कूल के पीछे कॉमर्शियल ब्लॉक में सी.सी. सड़क निर्माण कार्य।","SANJAY MEENA",14.97m,10.19m,"11/14/2024","3/13/2024","कार्य पूर्ण",null,"NS","BDA","BDA","NO","ROAD","M/S RAMESHWAR PRASAD SHARMA",null,9.42m),
            ("15/2025-26","एन.एच. 21 पर सोलंकी हॉस्पीटल के सामने पुलिस विभाग की आरक्षित खाली भूमि पर एवं अन्य स्थानों पर मिट्टी भर्त का कार्य।","NAVEEN GOYAL",4.81m,4.49m,"4/27/2025","5/26/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","MISC","M/S KRISHNA CONSTRUCTION",null,4.57m),
            ("44/2025-26","सरकूलर रोड के भीतरी क्षेत्र में टोटल स्टेशन सर्वे का कार्य।","ANOOP SINGH",4.92m,4.47m,"5/25/2025","6/24/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","MISC","M/S ADITYA ASSOCITES",null,null),
            ("40/2025-26","जिला क्लब भरतपुर स्विमिंग पूल में यूटिलिटी विकास कार्य","ASHISH KUMAR MAURYA",4.85m,4.37m,"5/25/2025","6/24/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","MISC","M/S FAMOUS FOUNTAINS",null,4.56m),
            ("42/2025-26","स्वीमिंग पूल के चारों ओर मिट्टी भर्त, लैण्ड स्केपिंग एवं घास लगाने का कार्य","ASHISH KUMAR MAURYA",4.97m,4.46m,"5/22/2025","6/21/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","PARK","M/S SOGARWAL CONSTRUCTION",null,4.68m),
            ("43/2025-26","लोहागढ स्टेडियम में कुस्ती दंगल मैदान भवन परिसर में मरम्मत कार्य","ASHISH KUMAR SINGH",4.99m,4.53m,"5/22/2025","6/21/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","BUILDING","M/S RADHE SHYAM CONTRACTOR",null,4.72m),
            ("31/2025-26","श्रीमान जिला कलक्टर एवं अध्यक्ष महोदय के कैम्प ऑफिस में पार्क में स्थापित लाइटों का मरम्मत कार्य।","DHARMENDRA",0.84m,0.76m,"5/11/2025","6/10/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","ELECTRIC","M/S PATHAK ELECTRICALS",null,0.79m),
            ("21/2025-26","महिला थाने से सारस चौराहे तक स्ट्रीट लाईट पोल शिफ्टिंग का कार्य।","DHARMENDRA",4.97m,4.84m,"4/30/2025","7/29/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","ELECTRIC","M/S PATHAK ELECTRICALS",null,null),
            ("48/2025-26","लोहागढ स्टेडियम में कुस्ती दंगल में लाइटिंग का कार्य","DHARMENDRA",4.97m,4.55m,"5/26/2025","6/25/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","ELECTRIC","M/S PATHAK ELECTRICALS",null,null),
            ("22/2025-26","प्राधिकरण आवास संख्या 02 में विद्युत सम्बन्धी मरम्मत कार्य","DHARMENDRA",2.34m,2.2m,"4/30/2025","7/29/2025","कार्य पूर्ण",null,"S","BDA","BDA","NO","ELECTRIC","M/S N & D INFRA",null,2.29m),
            ("39/2024-25","बृजेन्द्र बिहारी कुण्डा के पास पेट्रोल पम्प के पीछे व खसरा नं. 1909, 1910 की चार-दीवारी का कार्य।","LAV KUSH YADAV",26.18m,18.19m,"8/24/2024","12/23/2024","कार्य पूर्ण",null,"NS","BDA","BDA","NO","BUILDING","M/S RADHA RANI CONTRACTOR",null,14.85m),
            ("25/2025-26","भरतपुर विकास प्राधिकरण कार्यालय एवं जिला क्लब में थ्री डी साईन बोर्ड लगाने का कार्य","LAV KUSH YADAV",2.71m,2.54m,"5/2/2025","6/1/2025","कार्य पूर्ण",null,"S","BDA","BDA","NO","ELECTRIC","M/S GOODS & SERVICES",null,2.54m),
            ("45/2025-26","प्राधिकरण कार्यालय में विद्युत व्यवस्था हेतु कक्ष निर्माण एवं अन्य मरम्मत कार्य।","LAV KUSH YADAV",4.97m,4.5m,"5/25/2025","6/24/2025","कार्य पूर्ण",null,"S","BDA","BDA","NO","BUILDING","M/S KAPIL JAIN",null,null),
            ("46/2025-26","भरतपुर विकास प्राधिकरण कार्यालय में विभिन्न आवश्यक सैनेटरी एवं मरम्मत कार्य। (वार्षिक)","LAV KUSH YADAV",3.43m,3.11m,"5/25/2025","6/24/2025","कार्य पूर्ण",null,"S","BDA","BDA","NO","BUILDING","M/S KAPIL JAIN",null,null),
            ("47/2025-26","ऑडिटोरियम में वॉल पेन्टिंग प्लेटफॉर्म, नाली पर ग्रेटिंग लगाये जाने का कार्य","ASHISH KUMAR MAURYA",4.93m,4.48m,"5/26/2025","6/25/2025","कार्य पूर्ण",null,"S","BDA","BDA","NO","MISC","M/S RAJASTHAN AGRICULTURE",null,null),
            ("55/2025-26","एम.एस.जे. कॉलेज स्थित हनुमान मंदिर के निकट कुण्ड में निर्माण एवं आवश्यक मरम्मत कार्य।","NAVEEN GOYAL",4.97m,4.54m,"6/2/2025","7/1/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","MISC","M/S AVINASH KUMAR GOYAL",null,null),
            ("56/2025-26","एम.एस.जे. कॉलेज में हनुमान मंदिर के पास कुण्ड के चारों तरफ पाथवे पर डब्ल्यू.बी.एम. व सीढियों पर पत्थर लगाने का कार्य।","NAVEEN GOYAL",4.98m,4.55m,"6/6/2025","7/5/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","ROAD","M/S AVINASH KUMAR GOYAL",null,4.76m),
            ("20/2025-26","आयुक्त आवास स्थित कैम्प ऑफिस में मरम्मत कार्य।","NAVEEN GOYAL",4.99m,4.71m,"4/30/2025","5/29/2025","कार्य पूर्ण",null,"S","BDA","BDA","NO","BUILDING","M/S BHOLEY SHANKAR GARG",null,4.59m),
            ("58/2025-26","जिला क्लब भरतपुर में दो सिन्थेटिक बैडमिन्टन कोर्ट का निर्माण कार्य","ASHISH KUMAR MAURYA",4.88m,4.57m,"6/12/2025","7/11/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","MISC","M/S SUPREME SPORTS",null,null),
            ("65/2025-26","भरतपुर विकास प्राधिकरण के जल भराव क्षेत्र में आवश्यकतानुसार बाउण्ड्री वॉल व मिट्टी भर्त का कार्य।","ASHISH KUMAR MAURYA",4.93m,4.61m,"6/8/2025","7/7/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","MISC","M/S KRISHNA CONSTRUCTION",null,null),
            ("67/2025-26","अधिशाषी अभियंता तृतीय के क्षेत्र में पानी की लीकेज लाइन का मरम्मत कार्य।","ASHISH KUMAR MAURYA",4.99m,4.64m,"6/9/2025","7/8/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","MISC","M/S KRISHNA CONSTRUCTION",null,null),
            ("68/2025-26","डिस्ट्रिक क्लब स्विमिंग पूल में प्लास्टर एवं ग्रेनाइट लगाने का कार्य।","ASHISH KUMAR MAURYA",4.99m,4.49m,"6/9/2025","7/8/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","MISC","M/S FAMOUS FOUNTAIN",null,null),
            ("69/2025-26","जिला क्लब स्वीमिंग पूल के चारों तरफ डेक एरिया का निर्माण कार्य।","ASHISH KUMAR MAURYA",4.96m,4.46m,"6/8/2025","7/7/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","MISC","M/S FAMOUS FOUNTAIN",null,4.66m),
            ("70/2025-26","सर्किट हाउस में पाथवे पर मध्य में स्टील की रैलिंग व स्टील गेट लगाने का कार्य।","NAVEEN GOYAL",3.68m,3.51m,"6/5/2025","7/4/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","ROAD","M/S AVINASH KUMAR GOYAL",null,null),
            ("23/2025-26","आयुक्त निवास में आउटर डवलपमेंट का कार्य","NAVEEN GOYAL",4.91m,4.64m,"4/30/2025","5/29/2025","कार्य पूर्ण",null,"S","BDA","BDA","NO","BUILDING","M/S BHOLEY SHANKAR GARG",null,4.66m),
            ("123/2024-25","लोहागढ़ स्टेडियम में भवन मरम्मत कार्य।","LAV KUSH YADAV",14.9m,12.47m,"2/23/2024","4/22/2024","कार्य पूर्ण",null,"NS","BDA","BDA","NO","BUILDING","M/S RADHE SHYAM CONTRACTOR",null,10.08m),
            ("24/2024-25","न्यास की ट्रांसपोर्ट नगर योजना में भूखण्ड संख्या 520 के सामने सी.सी. सड़क निर्माण कार्य।","SANJAY MEENA",26.59m,16.96m,"8/9/2024","2/24/2024","कार्य पूर्ण",null,"S","BDA","BDA","NO","ROAD","M/S RAMESHWAR PRASAD SHARMA",null,15.53m),
            ("71/2025-26","स्वर्ण जयन्ती नगर योजना में टोटल स्टेषन (टी.एस.) सर्वे का कार्य।","SANTOSH KUMARI",4.99m,4.54m,"6/9/2025","7/8/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","MISC","M/S ADITYA ASSOCITES",null,4.72m),
            ("72/2025-26","वी.पी.एस. पार्क में झुले एंव फिसलपट्टी, मरम्मत का कार्य।","SANTOSH KUMARI",3.83m,3.14m,"6/9/2025","7/8/2025","कार्य पूर्ण",null,"S","BDA","BDA","NO","PARK","M/S RK SPORTS",null,null),
            ("73/2025-26","एम.एस.जे. कॉलेज स्थित कुण्ड में पाथवे पर मिट्टी डालने व आवष्यक मरम्मत कार्य।","NAVEEN GOYAL",4.96m,4.75m,"5/20/2025","6/19/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","ROAD","M/S AVINASH KUMAR GOYAL",null,4.55m),
            ("124/2024-25","लोहागढ स्टेडियम में 03 हाई मास्ट लाइट लगाने का कार्य।","DHARMENDRA",18.49m,11.38m,"2/23/2024","4/22/2024","कार्य पूर्ण",null,"NS","BDA","BDA","NO","ELECTRIC","M/S N&D INFRA",null,null),
            ("74/2024-36","राजकीय एम.एस.जे. कॉलेज परिसर में हनुमान मंदिर के पीछे कुण्डे का रेजूनेशन कार्य।","NAVEEN GOYAL",29.4m,23.86m,"9/27/2024","3/31/2024","कार्य पूर्ण",null,"NS","BDA","BDA","NO","MISC","M/S MAHENDRA KUMAR GOYAL",null,25.77m),
            ("49/2024-25","कलक्ट्रैट से सारस चौराहे तक सड़क के दोनो तरफ फुटपाथ निर्माण, नाला मरम्मत एंवं अन्य विकास कार्य।","NAVEEN GOYAL",99.42m,78m,"9/7/2024","3/31/2024","कार्य पूर्ण",null,"NS","BDA","BDA","NO","DRAIN","M/S A.K. INDUSTRIES",null,50.4m),
            ("75/2024-25","एम.एस.जे. कॉलेज ग्राउण्ड में भवन मरम्मत, खेलों हेतु विभिन्न विकास कार्य एवं ओपन जिम लगाने का कार्य।","NAVEEN GOYAL",85.6m,77.36m,"10/20/2024","2/19/2024","कार्य पूर्ण",null,"NS","BDA","BDA","NO","MISC","M/S SHYAM SUNDER SHARMA",null,47.21m),
            ("76/2025-26","जिला कलक्टर महोदय के कैम्प कार्यालय पर विद्युत संबंधी कार्य।","DHARMENDRA",4.86m,4.42m,"6/14/2025","7/13/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","ELECTRIC","M/S PATHAK ELECTRICALS",null,null),
            ("18/2025-26","जवाहर नगर स्थित कब्रिस्तान के सामने सरकारी खाली भूखण्ड में मिट्टी भर्त का कार्य","SANTOSH KUMARI",4.33m,4.1m,"4/30/2025","5/29/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","MISC","M/S PRADEEP PATHAK CONTRACTOR",null,null),
            ("01/2024-25","लोहागढ किले के अंदर व आसपास क्षेत्र में सौन्दर्यीकरण हेतु निर्माण कार्यों की डी.पी.आर. तैयार करने का कार्य।","ANOOP SINGH",60m,23.07m,"1/10/2024","2/28/2024","कार्य पूर्ण",null,"NS","BDA","LOAN","NO","MISC","M/S PDCOR LIMITED",null,null),
            ("141/2024-25","जसवंत प्रदर्शनी मेला ग्राउण्ड में जीर्णोद्धार एवं सौन्दर्यीकरण का कार्य।","ASHISH KUMAR SINGH",700m,559.23m,"3/28/2025","1/10/2026","85%",85m,"NS","BDA","SASCI","NO","MISC","M/S HARSWROOP SHARMA",null,null),
            ("129/2024-25","लोहागढ़ फोर्ट में लाइटिंग का कार्य।","DHARMENDRA",41.6m,25.87m,"2/29/2024","5/28/2024","कार्य पूर्ण",null,"NS","BDA","BDA","NO","ELECTRIC","M/S PATHAK ELECTRICALS",null,23.31m),
            ("74/2024-40","काली बगीची चौराहे से बीना महल (एन एच-21) तक सड़क डामरीकरण एवं सडक के दोनो तरफ इन्टरलॉकिंग टाइल्स तथा प्रोटेक्षन वॉल का निर्माण कार्य।","SANTOSH KUMARI",133.95m,89.71m,"9/7/2024","3/6/2024","कार्य पूर्ण",null,"NS","BDA","BDA","NO","ROAD","M/S V.K. CONSTRUCTION",null,null),
            ("68/2024-25","दीन दयाल नगर के निकट स्थित हनुमान कुण्ड का विकास कार्य।","ASHISH KUMAR MAURYA",60.96m,47.9m,"9/22/2024","3/21/2024","कार्य पूर्ण",null,"NS","BDA","BDA","NO","MISC","M/S JAGDISH SINGH UBAR",null,30.82m),
            ("75/2024-40","बी-नारायण गेट स्थित कच्चा कुण्डा का रेजूनेषन का कार्य।","SANTOSH KUMARI",99.35m,78.06m,"9/22/2024","3/21/2024","कार्य पूर्ण",null,"NS","BDA","BDA","NO","MISC","M/S VIJAYBHAN SINGH",null,33.48m),
            ("74/2024-42","वार्ड नं. 44 में संविदा भवन से शास्त्री पार्क होते हुए राजेन्द्र नगर रोड तक मय लिंक सड़को पर रि-कार्पेटिंग एवं सड़क किनारे इंटरलॉकिंग टाइल्स लगाने का कार्य।","SANTOSH KUMARI",42.85m,40.18m,"12/30/2024","3/1/2024","कार्य पूर्ण",null,"NS","BDA","BDA","NO","ROAD","M/S A.K. INDUSTRIES",null,null),
            ("78/2025-26","सोलंकी हॉस्पीटल के सामने सारस चौकी के लिए लैट-बाथ एवं अन्य आवश्यक मरम्मत कार्य","NAVEEN GOYAL",4.92m,4.7m,"6/16/2025","7/15/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","BUILDING","M/S MAHESH CHAND SATISH CHAND",null,null),
            ("75/2024-37","जिला कलैक्ट्रेट परिसर में स्थित सूचना केन्द्र में पुस्तकालय का निर्माण कार्य।","NAVEEN GOYAL",25m,20.82m,"1/31/2024","4/30/2024","कार्य पूर्ण",null,"NS","BDA","BDA","NO","BUILDING","M/S HANUMAN CONSTRUCTION",null,null),
            ("04/2025-26","राजकीय एम.एस.जे. कॉलेज ग्राउण्ड में निर्माणाधीन स्क्वैश कोर्ट में वूडन फ्लोरिंग का कार्य।","KAVITA BHATI",7.57m,6.37m,"4/17/2025","6/16/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","BUILDING","M/S MANOJ KUMAR JINDAL",null,3.05m),
            ("08/2025-26","जिला क्लब में लाइटिंग का कार्य।","DHARMENDRA",24.93m,15.32m,"4/25/2025","7/24/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","ELECTRIC","M/S ANJU ELECTRIC & SOUND SERVICE",null,14.73m),
            ("39/2025-26","प्राधिकरण कार्यालय नया भवन जन सुनवाई केन्द्र के निकट एवं कार्यालय भवन नये कमरों में ए.सी. लगाने एवं सर्विसिंग का कार्य","DHARMENDRA",10.01m,7.4m,"5/22/2025","8/21/2025","कार्य पूर्ण",null,"S","BDA","BDA","NO","ELECTRIC","M/S ANJU ELECTRIC & SOUND SERVICE",null,7.69m),
            ("63/2025-26-C","स्वर्ण जयंती नगर योजना में टोटल स्टेशन सर्वे का कार्य।","SANTOSH KUMARI",4.99m,4.54m,"6/9/2025","7/8/2025","कार्य पूर्ण",null,"S","BDA","BDA","NO","MISC","M/S ADITYA ASSOCIATES",null,4.53m),
            ("06/2025-26","राजकीय एम.एस.जे. कॉलेज ग्राउण्ड के महिला थाने वाले गेट से एस.पी. ऑफिस तक नाला मरम्मत एवं नाला ढकने का कार्य।","NAVEEN GOYAL",36.88m,27.16m,"4/25/2025","7/24/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","DRAIN","M/S P.C. CONSTRUCTION",null,13.86m),
            ("65/2024-25","पुलिस महानिरीक्षक राजकीय आवास कृष्णा नगर के निकट स्थित कुण्ड के सौन्दर्यीकरण का कार्य।","ASHISH KUMAR MAURYA",29.77m,23.56m,"9/27/2024","26/12/2024","कार्य पूर्ण",null,"NS","BDA","BDA","NO","WATER","M/S DEVANSHI CONTRACTOR","8619537003",null),
            ("46/2024-25","पुलिस अधीक्षक कार्यालय के निकट स्थित डिग्गी से राम नगर दो मोरा तक नाला निर्माण कार्य।","ASHISH KUMAR MAURYA",95.88m,70.75m,"9/7/2024","6/7/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","DRAIN","M/S VIJAYBHAN SINGH","8824593862",38.61m),
            ("27/2025-26","प्राधिकरण कार्यालय में विद्युत लोड बढ़ने के कारण एलटी पैनल एवं केबिल डालने का कार्य।","DHARMENDRA",13.97m,10.62m,"5/2/2025","7/1/2025","कार्य पूर्ण",null,"S","BDA","BDA","NO","ELECTRIC","M/S GOYAL ELECTRICALS",null,null),
            ("82/2024-25","न्यास की सैक्टर 03 योजना में 100 फीट सड़क पर चांदपोल गेट के सामने से अखड्ड रोड तक स्ट्रीट लाइट लगाने का कार्य।","DHARMENDRA",51.81m,35.07m,"10/25/2024","6/24/2025","कार्य पूर्ण",null,"S","BDA","BDA","YES","ELECTRIC","M/S PATHAK ELECTRICALS",null,36.21m),
            ("33/2024-25","शहर के विभिन्न पार्कों में मरम्मत एवं जीर्णोद्धार का कार्य।","LAV KUSH YADAV",92.12m,80.4m,"8/15/2024","5/14/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","PARK","M/S BAJRANG CONSTRUCTION",null,76.99m),
            ("88/2024-25","जिला क्लब भरतपुर में स्विमिंग पूल का निर्माण कार्य।","ASHISH KUMAR MAURYA",74.65m,67.47m,"11/28/2024","3/27/2025","कार्य पूर्ण",null,"NS","BDA","SASCI","NO","MISC","M/S FAMOUS FOUNTAINS","9214042714",null),
            ("47/2024-25","न्यास की ट्रांसपोर्ट नगर योजना फेज-2 में आर.एन.एफ.डी. नहर के ऊंपर आर.सी.सी. कल्वर्ट का निर्माण कार्य।","ASHISH KUMAR SINGH",98.95m,74.61m,"9/7/2024","3/24/2025","कार्य पूर्ण",null,"S","BDA","SASCI","NO","ROAD","M/S DEVANSHI CONTRACTOR",null,null),
            ("67/2024-25","अछनेरा रोड लक्ष्मी नगर स्थित डिग्गी के सौन्दर्यीकरण का कार्य।","ASHISH KUMAR MAURYA",48.86m,38.86m,"9/22/2024","3/21/2025","कार्य पूर्ण",null,"NS","BDA","SASCI","NO","WATER","M/S VIJAYBHAN SINGH","8824593862",null),
            ("104/2024-25","बृजेन्द्र बिहारी कुण्डा सेवर के सहारे बाहर की तरफ फुटपाथ का निर्माण कार्य।","LAV KUSH YADAV",39.34m,27.06m,"1/10/2025","3/9/2025","कार्य पूर्ण",null,"NS","BDA","SASCI","NO","MISC","M/S GANPATI CONSTRUCTION",null,21.34m),
            ("225/2023-24","न्यास की सैक्टर 03 योजना एवं मुखर्जी नगर योजना में पार्क की देख रेख, नियमित साफ-सफाई एवं बागवानी संधारण कार्य (वार्षिक अनुबंध)","SANJAY MEENA",6.98m,6.08m,"3/15/2024","3/14/2025","कार्य पूर्ण",null,"S","BDA","BDA","NO","MISC","M/S LOHAGARH GREEN NURSERY & WATER SUPPLIERS",null,1.54m),
            ("24/2025-26","बिजलीघर चौराहे से सारस चौराहा तक पेडों की कांट छांट का कार्य (वार्षिक)","NAVEEN GOYAL",1.99m,1.82m,"4/30/2025","5/29/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","MISC","M/S V P NURSERY",null,null),
            ("14/2025-26","महिला थाने से सारस चौराहे तक सडक की दोनो तरफ से पेड शिफ्टिंग एवं रख रखाब का कार्य।","NAVEEN GOYAL",4.98m,4.6m,"4/27/2025","5/26/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","MISC","M/S KRISHNA CONSTRUCTION",null,null),
            ("224/2023-24","माननीय मुख्यमंत्री महोदय के गृह जिले भरतपुर के जन-सुनवाई केन्द्र कार्यालय पर लॉन में घास लगाने एवं गौरव पथ पर पूर्व में लगे पेड पौधे की देख-रेख (वार्षिक अनुबंध)","SANTOSH KUMARI",4.5m,4.06m,"3/15/2024","3/14/2025","कार्य पूर्ण",null,"S","BDA","BDA","NO","PARK","M/S V.P. NURSERY",null,4.25m),
            ("54/2023-24","न्यास ऑडिटोरियम में स्टेज लाइटिंग, साउण्ड सिस्टम रख-रखाव एवं संचालन का कार्य। (वार्षिक)","DHARMENDRA",2.51m,2.28m,"7/27/2023","7/26/2024","कार्य पूर्ण",null,"S","BDA","BDA","NO","ELECTRIC","M/S N & D INFRA",null,null),
            ("123/2023-24","वी.पी.एस. पार्क में म्यूजिकल फाउन्टेन की देखरेख, मरम्मत एवं संचालन का कार्य। (वार्षिक अनुबंध)","DHARMENDRA",3.76m,3.36m,"9/28/2023","9/27/2024","कार्य पूर्ण",null,"S","BDA","BDA","NO","ELECTRIC","M/S N & D INFRA",null,1.92m),
            ("150/2023-24","न्यास क्षेत्र के अंतर्गत न्यास चौराहा, यातायात तिराहा एवं हीरादास चौराहा पर स्थित फब्बारों की देख-रेख, मरम्मत एवं संचालन का कार्य। (वार्षिक)","DHARMENDRA",3.13m,2.4m,"10/8/2023","10/7/2024","कार्य पूर्ण",null,"NS","BDA","BDA","NO","ELECTRIC","M/S N & D INFRA",null,2.2m),
            ("107/2025-26","श्रीमान जिला कलक्टर महोदय के कैम्प कार्यालय में आवश्यक मरम्मत कार्य","NAVEEN GOYAL",4.97m,4.57m,"7/21/2025","8/20/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","BUILDING","M/S KAPIL JAIN CONTRACTOR",null,null),
            ("111/2025-26","बृजेन्द्र बिहारी कुण्डे के पास स्थित बुर्ज पर इन्टरलॉकिंग पाथवे निर्माण कार्य","LAV KUSH YADAV",4.96m,4.51m,"7/25/2025","8/24/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","BUILDING","M/S UTKARSH CONSTRUCTION",null,null),
            ("115/2025-26","सारस चौराहा से महिला थाने तक सड़क के एक तरफ सीवरेज आई.सी. चैम्बर एवं पाइप को नीचे करने का कार्य।","NAVEEN GOYAL",2.05m,1.93m,"27.07.2025","26.08.2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","MISC","M/S KRISHNA CONSTRUCTION",null,1.92m),
            ("108/2025-26","एल.बी. शास्त्री नगर में सर्वे एवं अन्य कार्य","LAV KUSH YADAV",4.98m,4.52m,"7/21/2025","8/20/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","MISC","M/S ADITYA ASSOCIATES",null,4.39m),
            ("116/2025-26","प्राधिकरण के आवास संख्या 02 में फर्नीचर एवं मरम्मत कार्य।","NAVEEN GOYAL",5m,4.54m,"25.07.2025","24.08.2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","BUILDING","M/S BHOLE SHANKAR",null,null),
            ("52/2025-26","राधिका विहार कॉलोनी में मकान नं. 128 से मकान नं. 28 तक नाला निर्माण कार्य।","NAVEEN GOYAL",9.67m,7.4m,"6/1/2025","7/30/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","DRAIN","M/S RAHUL CONSTRUCTION",null,null),
            ("117/2025-26","लुपिन चौराहे से नहर तक आर.सी.सी. बॉक्स कल्बर्ट डालने का कार्य।","ASHISH KUMAR MAURYA",4.99m,4.51m,"07.08.2025","06.09.2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","DRAIN","M/S VIJAYBHAN SINGH",null,null),
            ("02/2024-25","न्यास क्षेत्र में विभिन्न स्थानों पर फेरो कवर सप्लाई एवं लगाने का कार्य। (वार्षिक अनुबंध)","NAVEEN GOYAL",9.89m,7.55m,"6/23/2024","6/22/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","MISC","M/S LOHAGARH GREEN NURSERY & WATER SUPPLIERS",null,null),
            ("119/2025-26","प्राधिकरण क्षेत्र में शीशम तिराहा वाया सेवर वाया हीरादास बस स्टैण्ड से शीशम तिराहे तक पौधारोपण हेतु पौधा सप्लाई एवं गडडे खोदने का कार्य।","ASHISH KUMAR SINGH",4.91m,4.6m,"08.08.2025","07.09.2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","MISC","M/S V.P. NURSERY",null,4.12m),
            ("120/2025-26","जिला परिषद कार्यालय से रामनगर दो मोरा नाले तक आर.सी.सी. बॉक्स कल्वर्ट डालने का कार्य।","ASHISH KUMAR MAURYA",4.96m,4.49m,"08.08.2025","07.09.2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","DRAIN","M/S VIJAYBHAN SINGH",null,null),
            ("64/2025-26","जवाहर नगर कब्रिस्तान से कच्चा कुण्डा तक पाईप लाईन डालने का कार्य।","SANTOSH",18.22m,14.82m,"6/13/2025","8/12/2025","कार्य पूर्ण",null,"S","BDA","BDA","NO","MISC","M/S SOGARWAL CONSTRUCTION",null,null),
            ("29/2024-25","कलैक्टेªट के अंदर व बाहर पार्क, न्यास चौराहे पर सर्किल, आइलैण्ड न्यास कार्यालय एवं अन्य में लोन की देख-रेख एवं संधारण का कार्य। (वार्षिक अनुबन्ध)","NAVEEN GOYAL",5m,3.73m,"8/15/2024","8/14/2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","MISC","M/S LOHAGARH GREEN NURSERY & WATER SUPPLIERS",null,null),
            ("126/2025-26","सरसों अनुसंधान केन्द्र से अखड्ड तिराहा वॉया कुम्हेर गेट से हीरादास बस स्टैण्ड होते हुए खड्डे खोदने, पौधा सप्लाई एवं पौधारोपण कार्य।","ASHISH KUMAR SINGH",4.92m,4.48m,"29.08.2025","28.09.2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","MISC","M/S V P NURSERY",null,null),
            ("127/2025-26","एम.एस.जे. कॉलेज ग्राउण्ड में यू शैप कल्वर्ट को फिक्स करने का कार्य।","NAVEEN GOYAL",5m,4.55m,"30.08.2025","29.08.2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","MISC","M/S KRISHNA CONDTRUCTION",null,null),
            ("128/2025-26","विकास प्राधिकरण की एस.पी.जेड. योजना के एफ ब्लॉक तथा सड़क किनारे एवं डिवाइडर पर खड्डे खोदने, पौधा सप्लाई एवं पौधारोपण कार्य।","NARESH SINGH",4.84m,4.38m,"30.08.2025","29.09.2025","कार्य पूर्ण",null,"S","BDA","BDA","NO","MISC","M/S KRISHNA CONDTRUCTION",null,null),
            ("130/2025-26","अछनेरा रोड से काली बगीची श्मशान रोड तक खड्डे खोदने, पौधा सप्लाई एवं पौधारोपण कार्य।","NAVEEN GOYAL",4.85m,4.43m,"04.09.2025","03.10.2025","कार्य पूर्ण",null,"NS","BDA","BDA","NO","MISC","M/S SGARWAL CONSTRUCTION",null,null),
            ("62/2024-25","रेड क्रॉस सर्किल के निकट स्थित इन्द्रा पार्क में बाउन्ड्री वॉल निर्माण, रेलिंग लगाने एवं मिट्टी भर्त का कार्य।","ANOOP SINGH",99.33m,71.27m,"9/27/2024","7/8/2025","कार्य पूर्ण",null,"NS","TOURISM","TOURISM","YES","PARK","M/S MANOJ JINDAL",null,58.89m),
            ("28/2023-24","न्यास ऑडिटोरियम में साफ-सफाई का कार्य (वार्षिक अनुबन्ध)","NARESH SINGH",2.76m,2.26m,"6/26/2023","6/25/2025","IN PROGRESS",null,"NS","BDA","BDA","NO","MISC","M/S V.P. NURSERY",null,2.34m),
            ("223/2023-24","न्यास ऑडिटोरियम पार्क में पौधारोपण, देखरेख एवं बागवानी संधारण का कार्य। (वार्षिक अनुबंध)","NARESH SINGH",1.46m,1.21m,"3/15/2024","3/14/2025","IN PROGRESS",null,"S","BDA","BDA","NO","MISC","M/S V.P. NURSERY",null,1.23m),
            ("74/2024-41","विकास प्राधिकरण क्षेत्र में विभिन्न स्थानों पर सडकों के एल सैक्शन एवं क्रॉस सैक्शन मय ड्राइंग बनाने का कार्य।","SANTOSH",4.96m,4.49m,"1/30/2025","1/29/2026",null,null,"NS","BDA","BDA","NO","MISC","M/S ADITYA ASSOCIATES",null,null),
            ("210/2023-24","वी.पी.एस. पार्क में पौधारोपण, बागवानी एवं संचालन का कार्य। (वार्षिक अनुबंध)","SANTOSH",14.3m,6.98m,"3/15/2024","3/14/2025","IN PROGRESS",null,"S","BDA","BDA","NO","PARK","M/S V P NURSERY",null,6.48m),
            ("21/2024-25","अछनेरा रोड से स्टेषन रोड के मध्य क्षेत्र में विभिन्न स्थानों पर नाली/नाला क्रॉस एवं जाल निर्माण तथा पेच मरम्मत कार्य। (वार्षिक अनुबंध)","ASHISH KUMAR MAURYA",9.63m,7.83m,"7/29/2024","7/28/2025","IN PROGRESS",null,"NS","BDA","BDA","NO","DRAIN","M/S SOGARWAL CONSTRUCTION","9694660971",1.98m),
            ("22/2024-25","न्यास की एस.पी.जैड. योजना में ब्लॉक ए एवं एच में स्थित पार्कों में पेड पौधों की देखरेख, साफ सफाई एवं संधारण कार्य। (वार्षिक अनुबंध)","NARESH SINGH",3.7m,2.69m,"8/22/2024","8/21/2025","IN PROGRESS",null,"S","BDA","BDA","NO","PARK","M/S SOGARWAL CONSTRUCTION",null,1.37m),
            ("69/2024-25","एस.पी. ऑफिस के सामने स्थित एस.पी.जे. कुण्ड का सौन्दर्यीकरण कार्य।","ASHISH KUMAR MAURYA",76.64m,65.35m,"9/27/2024","6/15/2025","100%",100m,"NS","BDA","SASCI","NO","WATER","M/S VIJAYBHAN SINGH","8824593862",null),
            ("147/2024-25","एम.एस.जे. कॉलेज से घासीराम तिराहे तक विभिन्न स्थानों पर पेड शिफ्टिंग, कटिंग एवं रख रखाव का कार्य।","ASHISH KUMAR MAURYA",4.97m,4.73m,"4/5/2025","6/4/2025","100%",100m,"NS","BDA","BDA","NO","MISC","M/S GREEN EARTH ENTERPRISES","9873658423",null),
            ("118/2025-26","जसवंत प्रदर्शनी मेला ग्राउण्ड में सी.सी. सड़क निर्माण कार्य।","ASHISH KUMAR SINGH",27.55m,18.39m,"04.08.2025","03.11.2025","100%",100m,"NS","BDA","BDA","NO","ROAD","M/S R.K CONSTRUCTION",null,null),
            ("12/2025-26","सैक्टर नं. 03, 100 फीट रोड की ओवर हैड विद्युत लाइन शिफ्टिंग का कार्य।","LAV KUSH YADAV",38.52m,31.74m,"4/27/2025","7/26/2025","100%",100m,"S","BDA","BDA","NO","ELECTRIC","M/S BAJRANG CONSTRUCTION (ELECT)",null,null),
            ("94/2025-26","जिला कलैक्ट्रेट मीटिंग हॉल में विद्युत फिटिंग का कार्य।","DHARMENDRA",6.02m,4.32m,"7/4/2025","12/11/2025","100%",100m,"NS","BDA","BDA","NO","ELECTRIC","M/S SHREE GOVIND ENTERPRIZES",null,null),
            ("51/2024-25","न्यास की कॉमर्षियल योजना ट्रांसपोर्ट नगर में 80 फीट रोड पर नाली निर्माण कार्य।","ASHISH KUMAR SINGH",131.79m,88.27m,"9/7/2024","7/15/2025","100%",100m,"S","BDA","BDA","NO","DRAIN","M/S RAMESHWAR PRASAD SHARMA",null,null),
            ("79/2024-25","केवलादेव राष्ट्रीय उद्यान भरतपुर में केवलादेव चौकी से सांखला हैड एवं सांपन मोरी से ट्रेन नं. 03 तक इंटरलॉकिंग टाइल्स लगाने का कार्य।","NARESH",161.76m,97.96m,"10/25/2024","7/31/2025","100%",100m,"NS","FOREST","DEPOSIT","NO","ROAD","M/S A.K. INDUSTRIES","9828345950",null),
            ("27/2024-25-C","विश्वप्रिय शास्त्री पार्क में विभिन्न प्रकार के आवष्यक मरम्मत एवं पेटिंग संबंधी कार्य।","SANTOSH",29.96m,23.55m,"2/23/2025","5/22/2025","100%",100m,"S","BDA","BDA","NO","BEAUTIFY","M/S AVINASH KUMAR GOYAL","9414268338",null),
            ("53/2025-26","सलूजा नर्सिंग होम वाली सडक से सी.एम. हाउस तक कब्रिस्तान की बाउण्ड्रीवॉल का निर्माण कार्य।","SANTOSH",36.56m,26.78m,"6/1/2025","8/31/2025","100%",100m,"NS","BDA","BDA","NO","MISC","M/S PAVANI CONSTRUCTION","8963093151",null),
            ("89/2025-26","जिला क्लब में पैदल पथ का निर्माण कार्य","ASHISH KUMAR MAURYA",4.97m,3.82m,"7/4/2025","8/3/2025","Work Withdrawal",null,"NS","BDA","SASCI","NO","MISC","M/S S S INFRA","8529575573",null),
        };

        foreach (var c in completed)
        {
            var ph   = c.Ph;
            var stat = CompStatus(c.StatusRaw, ph);
            var work = new Work
            {
                WorkCode                 = UniqueCode(c.No),
                WorkName                 = c.Name.Trim(),
                AssignedJenId            = JenId(c.Jen),
                CategoryId               = CatId(c.Cat),
                FundSourceId             = FsId(c.Fund),
                SanctionedAmount         = c.San,
                AgreementAmount          = c.Agr,
                Expenditure              = c.Exp,
                StartDate                = ParseDate(c.St),
                ExpectedCompletion       = ParseDate(c.En),
                ProgressPercent          = (stat == WorkStatus.COMPLETED && ph == null) ? 100m : ph,
                Status                   = stat,
                IsScheme                 = IsScheme(c.Sc),
                IsCmBudget               = IsCm(c.Cm),
                Department               = string.IsNullOrWhiteSpace(c.Dept) ? null : c.Dept.Trim(),
                ContractorName           = string.IsNullOrWhiteSpace(c.Firm) ? null : c.Firm.Trim(),
                ContractorMobile         = string.IsNullOrWhiteSpace(c.Mob)  ? null : c.Mob.Trim(),
            };
            db.Works.Add(work);
        }
        await db.SaveChangesAsync();
    }
}
