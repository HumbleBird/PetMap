SET XACT_ABORT ON;
IF COL_LENGTH('dbo.Places', 'Phone') IS NULL
    THROW 51000, 'Deploy the latest PetMap schema before importing place rows.', 1;
BEGIN TRANSACTION;

-- Add only; existing rows are left unchanged.
IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'메종페퍼' AND (Address = N'경기도 안양시 동안구 호계동 번지 1층 1070-9' OR RoadAddress = N'경기 안양시 동안구 신기대로33번길 8'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'메종페퍼', N'경기도 안양시 동안구 호계동 번지 1층 1070-9', N'경기 안양시 동안구 신기대로33번길 8', 37.3804746637171, 126.958582393902, N'BakeryCafe', N'010-5815-0468', N'안내 공지 확인 가능 *무게 제한 X');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'아카루이 커피 로스터스' AND (Address = N'경기 안양시 동안구 시민대로 252 평촌푸르지오센트럴파크 2층 214호,246호' OR RoadAddress = N'경기 안양시 동안구 시민대로 252'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'아카루이 커피 로스터스', N'경기 안양시 동안구 시민대로 252 평촌푸르지오센트럴파크 2층 214호,246호', N'경기 안양시 동안구 시민대로 252', 37.39332788415504, 126.95893330848729, N'BakeryCafe', N'0507-1300-1681', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'수다' AND (Address = N'경기 안양시 동안구 관악대로342번길 28 1층' OR RoadAddress = N'경기 안양시 동안구 관악대로342번길 28'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'수다', N'경기 안양시 동안구 관악대로342번길 28 1층', N'경기 안양시 동안구 관악대로342번길 28', 37.402290167064, 126.961065292274, N'BakeryCafe', N'0507-1353-4694', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'크크커피' AND (Address = N'경기 안양시 만안구 박달우회로 156 1F, 2F' OR RoadAddress = N'경기 안양시 만안구 박달우회로 156'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'크크커피', N'경기 안양시 만안구 박달우회로 156 1F, 2F', N'경기 안양시 만안구 박달우회로 156', 37.4079690002882, 126.911877787411, N'BakeryCafe', N'0507-1378-7668', N'안내문 확인 가능 *1층만 가능');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'다르히 서비스샵' AND (Address = N'경기 안양시 만안구 만안로 241-5 동진빌딩 2층 201호' OR RoadAddress = N'경기 안양시 만안구 만안로 241-5'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'다르히 서비스샵', N'경기 안양시 만안구 만안로 241-5 동진빌딩 2층 201호', N'경기 안양시 만안구 만안로 241-5', 37.4015334646143, 126.921470664809, N'BakeryCafe', N'0507-1371-2984', N'안내문 확인 가능');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'에삐케이크' AND (Address = N'경기 안양시 만안구 양화로135번길 14 1F, B1' OR RoadAddress = N'경기 안양시 만안구 양화로135번길 14'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'에삐케이크', N'경기 안양시 만안구 양화로135번길 14 1F, B1', N'경기 안양시 만안구 양화로135번길 14', 37.4045388315351, 126.908342078457, N'BakeryCafe', N'0507-1300-5682', N'안내문 확인 가능 및 시설 확인 가능 *맹견 X');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'아에이오우' AND (Address = N'경기 안양시 동안구 갈산로 82 102호' OR RoadAddress = N'경기 안양시 동안구 갈산로 82'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'아에이오우', N'경기 안양시 동안구 갈산로 82 102호', N'경기 안양시 동안구 갈산로 82', 37.379128031693, 126.959739311011, N'BakeryCafe', NULL, N'안내문 확인 가능');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'카페 로린' AND (Address = N'경기 안양시 동안구 관양로 201-1 1층 101호' OR RoadAddress = N'경기 안양시 동안구 관양로 201-1'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'카페 로린', N'경기 안양시 동안구 관양로 201-1 1층 101호', N'경기 안양시 동안구 관양로 201-1', 37.4027396071485, 126.969722162649, N'BakeryCafe', N'0507-1423-7232', N'애견 동반 안내문 확인 가능');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'소네' AND (Address = N'경기도 안양시 동안구 관양로 73(1층 관양동)' OR RoadAddress = N'경기 안양시 동안구 관양로 73'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'소네', N'경기도 안양시 동안구 관양로 73(1층 관양동)', N'경기 안양시 동안구 관양로 73', 37.4019529635467, 126.955340431516, N'BakeryCafe', N'0507-1320-8678', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'우화' AND (Address = N'경기도 안양시 만안구 양화로72번길 83-21(1층 안양동)' OR RoadAddress = N'경기 안양시 만안구 양화로72번길 83-21'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'우화', N'경기도 안양시 만안구 양화로72번길 83-21(1층 안양동)', N'경기 안양시 만안구 양화로72번길 83-21', 37.4002025514072, 126.91864969660887, N'BakeryCafe', N'0507-1325-4260', N'*대형견,맹견 금지');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'술풀' AND (Address = N'경기도 안양시 동안구 평촌대로223번길 16(현대프라자 303호 호계동)' OR RoadAddress = N'경기 안양시 동안구 평촌대로223번길 16'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'술풀', N'경기도 안양시 동안구 평촌대로223번길 16(현대프라자 303호 호계동)', N'경기 안양시 동안구 평촌대로223번길 16', 37.391362995322, 126.954661252455, N'BakeryCafe', N'0507-1430-6845', N'안내문 확인 가능');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'슈퍼파슬리' AND (Address = N'경기 안양시 동안구 평촌대로223번길 11 성우프라자 2층 슈퍼파슬리' OR RoadAddress = N'경기 안양시 동안구 평촌대로223번길 11'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'슈퍼파슬리', N'경기 안양시 동안구 평촌대로223번길 11 성우프라자 2층 슈퍼파슬리', N'경기 안양시 동안구 평촌대로223번길 11', 37.39096483408, 126.954898636038, N'BakeryCafe', N'0507-1345-0167', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'카페조각' AND (Address = N'경기 안양시 동안구 평촌대로217번길 25 동화프라자 203호 2층 카페조각' OR RoadAddress = N'경기 안양시 동안구 평촌대로217번길 25'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'카페조각', N'경기 안양시 동안구 평촌대로217번길 25 동화프라자 203호 2층 카페조각', N'경기 안양시 동안구 평촌대로217번길 25', 37.3904375675349, 126.954459674347, N'BakeryCafe', N'0507-1320-3613', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'카페 크림스톤' AND (Address = N'경기 안양시 동안구 평촌대로 485 1층부터 3층' OR RoadAddress = N'경기 안양시 동안구 평촌대로 485'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'카페 크림스톤', N'경기 안양시 동안구 평촌대로 485 1층부터 3층', N'경기 안양시 동안구 평촌대로 485', 37.413680686248, 126.950907708234, N'BakeryCafe', N'0507-1345-9175', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'폼폼카페' AND (Address = N'경기 안양시 만안구 양화로28번길 89 1층 106호' OR RoadAddress = N'경기 안양시 만안구 양화로28번길 89'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'폼폼카페', N'경기 안양시 만안구 양화로28번길 89 1층 106호', N'경기 안양시 만안구 양화로28번길 89', 37.3993790781602, 126.91743650088297, N'BakeryCafe', N'0507-1389-3561', N'노키즈존');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'오늘하루' AND (Address = N'경기도 안양시 만안구 삼막로 38(지상1층 석수동)' OR RoadAddress = N'경기 안양시 만안구 삼막로 38'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'오늘하루', N'경기도 안양시 만안구 삼막로 38(지상1층 석수동)', N'경기 안양시 만안구 삼막로 38', 37.422839618096226, 126.91215183500817, N'BakeryCafe', N'0507-1363-4061', N'안내문 확인 가능');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'갈릭레몬' AND (Address = N'경기 안양시 동안구 동편로183번길 12 1층 갈릭레몬' OR RoadAddress = N'경기 안양시 동안구 동편로183번길 12'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'갈릭레몬', N'경기 안양시 동안구 동편로183번길 12 1층 갈릭레몬', N'경기 안양시 동안구 동편로183번길 12', 37.4066402482889, 126.970205144867, N'BakeryCafe', N'070-8657-2033', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'위드독' AND (Address = N'경기 안양시 만안구 예술공원로 196 세븐일레븐2층 위드독애견카페' OR RoadAddress = N'경기 안양시 만안구 예술공원로 196'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'위드독', N'경기 안양시 만안구 예술공원로 196 세븐일레븐2층 위드독애견카페', N'경기 안양시 만안구 예술공원로 196', 37.4197037256956, 126.927587027399, N'BakeryCafe', N'0507-1316-8114', N'애견카페');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'더홈' AND (Address = N'경기 안양시 만안구 삼막로 43 1층' OR RoadAddress = N'경기 안양시 만안구 삼막로 43'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'더홈', N'경기 안양시 만안구 삼막로 43 1층', N'경기 안양시 만안구 삼막로 43', 37.423707412190446, 126.91230559658065, N'BakeryCafe', N'0507-1490-5157', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'힌터하우스' AND (Address = N'경기 안양시 만안구 예술공원로 237 1층' OR RoadAddress = N'경기 안양시 만안구 예술공원로 237'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'힌터하우스', N'경기 안양시 만안구 예술공원로 237 1층', N'경기 안양시 만안구 예술공원로 237', 37.4189600713701, 126.931598094099, N'BakeryCafe', N'0507-1372-7926', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'꾸어바666' AND (Address = N'경기 안양시 동안구 평촌대로 492' OR RoadAddress = N'경기 안양시 동안구 평촌대로 492'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'꾸어바666', N'경기 안양시 동안구 평촌대로 492', N'경기 안양시 동안구 평촌대로 492', 37.41422414928722, 126.95126656800672, N'Restaurant', N'0507-1302-1265', N'전용층과 야외테이블에서 이용 가능 *대형견 및 애견 3마리 이상 방문시 산전 협의 후 이용 가능');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'삼동소바 안양평촌점' AND (Address = N'경기 안양시 동안구 평촌대로 474' OR RoadAddress = N'경기 안양시 동안구 평촌대로 474'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'삼동소바 안양평촌점', N'경기 안양시 동안구 평촌대로 474', N'경기 안양시 동안구 평촌대로 474', 37.41284780006453, 126.9522671420276, N'Restaurant', N'0507-1333-5526', N'겨울철(12월~2월) 한정');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'금수강산' AND (Address = N'경기 안양시 만안구 예술공원로208번길 72 1층' OR RoadAddress = N'경기 안양시 만안구 예술공원로208번길 72'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'금수강산', N'경기 안양시 만안구 예술공원로208번길 72 1층', N'경기 안양시 만안구 예술공원로208번길 72', 37.41814935733783, 126.93194902730818, N'Restaurant', N'0507-1492-2227', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'촌골오리' AND (Address = N'경기 안양시 만안구 예술공원로 247' OR RoadAddress = N'경기 안양시 만안구 예술공원로 247'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'촌골오리', N'경기 안양시 만안구 예술공원로 247', N'경기 안양시 만안구 예술공원로 247', 37.4186434492806, 126.932531490732, N'Restaurant', N'031-474-5292', N'*소형견 1마리 (순한 경우 중형견도 가능)');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'스시준' AND (Address = N'경기 안양시 만안구 태평로60번길 42 3층 307호' OR RoadAddress = N'경기 안양시 만안구 태평로60번길 42'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'스시준', N'경기 안양시 만안구 태평로60번길 42 3층 307호', N'경기 안양시 만안구 태평로60번길 42', 37.3963331598368, 126.930746814328, N'Restaurant', N'010-8159-5698', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'나빌리오' AND (Address = N'경기 안양시 만안구 안양로 125' OR RoadAddress = N'경기 안양시 만안구 안양로 125'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'나빌리오', N'경기 안양시 만안구 안양로 125', N'경기 안양시 만안구 안양로 125', 37.3857332747608, 126.931858641836, N'Restaurant', N'0507-1427-6892', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'명명식당' AND (Address = N'경기 안양시 만안구 경수대로1201번길 32 1층 명명식당' OR RoadAddress = N'경기 안양시 만안구 경수대로1201번길 32'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'명명식당', N'경기 안양시 만안구 경수대로1201번길 32 1층 명명식당', N'경기 안양시 만안구 경수대로1201번길 32', 37.4160500028042, 126.912769767171, N'Restaurant', N'010-8898-9923', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'꽃과국수' AND (Address = N'경기 안양시 만안구 냉천로31번길 26 지하1층' OR RoadAddress = N'경기 안양시 만안구 냉천로31번길 26'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'꽃과국수', N'경기 안양시 만안구 냉천로31번길 26 지하1층', N'경기 안양시 만안구 냉천로31번길 26', 37.384397275279, 126.926216321552, N'Restaurant', N'0507-1487-8896', N'소형견,테라스입장만 가능');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'뽁식당 범계점' AND (Address = N'경기 안양시 동안구 평촌대로223번길 49 아트타워빌딩 지하2층' OR RoadAddress = N'경기 안양시 동안구 평촌대로223번길 49'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'뽁식당 범계점', N'경기 안양시 동안구 평촌대로223번길 49 아트타워빌딩 지하2층', N'경기 안양시 동안구 평촌대로223번길 49', 37.3903541250168, 126.953057200063, N'Restaurant', N'010-6277-0855', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'야간매점' AND (Address = N'경기 안양시 만안구 장내로 101 지상1층' OR RoadAddress = N'경기 안양시 만안구 장내로 101'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'야간매점', N'경기 안양시 만안구 장내로 101 지상1층', N'경기 안양시 만안구 장내로 101', 37.3959528874278, 126.919312689658, N'Restaurant', N'010-6293-3439', N'소형견만 가능');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'안양본동물의료센터' AND (Address = N'경기 안양시 동안구 시민대로 370 안양본동물의료센터' OR RoadAddress = N'경기 안양시 동안구 시민대로 370'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'안양본동물의료센터', N'경기 안양시 동안구 시민대로 370 안양본동물의료센터', N'경기 안양시 동안구 시민대로 370', 37.39551205999431, 126.97152295466944, N'Hospital', N'031-396-8275', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'24시 비타민동물메디컬센터' AND (Address = N'경기 안양시 동안구 관악대로 267 1층, 2층' OR RoadAddress = N'경기 안양시 동안구 관악대로 267'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'24시 비타민동물메디컬센터', N'경기 안양시 동안구 관악대로 267 1층, 2층', N'경기 안양시 동안구 관악대로 267', 37.4026916376666, 126.952606767445, N'Hospital', N'031-465-7582', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'24시 이오동물병원' AND (Address = N'경기 안양시 동안구 관악대로 177 2층' OR RoadAddress = N'경기 안양시 동안구 관악대로 177'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'24시 이오동물병원', N'경기 안양시 동안구 관악대로 177 2층', N'경기 안양시 동안구 관악대로 177', 37.39921612742172, 126.94376252323313, N'Hospital', N'0507-1377-2575', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'24시 라이프동물메디컬센터' AND (Address = N'경기 안양시 만안구 안양로 249 디오르나인 2층' OR RoadAddress = N'경기 안양시 만안구 안양로 249'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'24시 라이프동물메디컬센터', N'경기 안양시 만안구 안양로 249 디오르나인 2층', N'경기 안양시 만안구 안양로 249', 37.3950532113123, 126.924065821262, N'Hospital', N'031-360-8279', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'학운공원' AND (Address = N'경기 안양시 동안구 학의로 71' OR RoadAddress = N'경기 안양시 동안구 비산동 1100'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'학운공원', N'경기 안양시 동안구 학의로 71', N'경기 안양시 동안구 비산동 1100', 37.3997590748783, 126.94890648012, N'Park', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'안양예술공원' AND (Address = N'경기 안양시 만안구 예술공원로131번길 7' OR RoadAddress = N'경기 안양시 만안구 예술공원로 131'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'안양예술공원', N'경기 안양시 만안구 예술공원로131번길 7', N'경기 안양시 만안구 예술공원로 131', 37.41906976861824, 126.91977026707937, N'Park', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = N'삼막애견공원' AND (Address = N'경기 안양시 만안구 석수동 18' OR RoadAddress = N'경기 안양시 만안구 석수동 18'))
BEGIN
    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)
    VALUES (N'삼막애견공원', N'경기 안양시 만안구 석수동 18', N'경기 안양시 만안구 석수동 18', 37.428932481227584, 126.9148675964867, N'Park', NULL, NULL);
END;

COMMIT TRANSACTION;
