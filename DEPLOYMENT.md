# PetMap 24/7 Linux 배포

현재 앱은 EF Core SQL Server provider와 SQL Server LocalDB 설정을 사용합니다. 데이터베이스 provider를 바꾸지 않고 배포할 수 있도록 앱, SQL Server Express, Caddy를 Docker Compose로 한 인스턴스에 올립니다.

## 권장 시작 구성과 비용

- 서울 리전의 **x86-64 Ubuntu 24.04 Lightsail 4GB**부터 시작합니다. 현재 공개 요금표 기준 4GB Linux 인스턴스는 월 **$24**, 전송량 4TB 포함입니다. Lightsail 지역별 콘솔 금액과 세금, 스냅샷 요금은 별도 확인하세요. 메모리 부족이 보이면 8GB 월 $44 요금제로 올립니다.
- SQL Server Linux 컨테이너는 Intel/AMD x86-64 호스트가 필요합니다. ARM/Graviton 인스턴스는 이 Compose 구성에 사용하지 마세요.
- SQL Server Express는 운영에 무료로 사용할 수 있지만, SQL Server 2022 기준 데이터베이스당 크기 제한은 10GB입니다.
- Lightsail 인스턴스는 한 대이므로 호스트 장애나 재부팅 중에는 사이트가 잠시 중단됩니다. Compose가 프로세스를 재시작해 주지만 고가용성 구성은 아닙니다.

## 1. Lightsail 인스턴스 준비

Lightsail에서 서울 리전을 선택하고 Linux/Unix의 **Ubuntu 24.04 x86-64** OS 전용 인스턴스를 만듭니다. 월 24달러 4GB 플랜을 선택하고 고정 IP를 인스턴스에 연결합니다.

Lightsail 네트워킹 방화벽에는 아래 포트만 추가합니다.

- TCP 80: 모든 IPv4/IPv6 (HTTP 및 인증서 발급)
- TCP 443: 모든 IPv4/IPv6 (HTTPS)
- TCP 22: 본인 관리 IP만 허용. 브라우저 SSH를 쓰면 외부 공개 SSH 규칙은 제거합니다.

SQL Server 1433과 앱 8080은 공개하지 않습니다. Lightsail 스냅샷 탭에서 자동 스냅샷을 켜고, 실제 복구가 가능한지도 주기적으로 확인합니다.

도메인의 DNS A 레코드를 인스턴스 고정 IP로 연결합니다. Caddy가 도메인에 대한 TLS 인증서를 자동 발급·갱신하므로, DNS 전파 후 80/443 접근이 가능해야 합니다.

## 2. Docker Engine 설치

SSH로 접속한 뒤 Docker의 Ubuntu 공식 apt 저장소에서 Engine과 Compose 플러그인을 설치합니다. [Docker 공식 설치 절차](https://docs.docker.com/engine/install/ubuntu/)를 따르세요. 설치 후 다음 명령이 동작해야 합니다.

```bash
sudo docker --version
sudo docker compose version
```

Docker 그룹은 root 권한과 동등하므로 사용자 계정을 그룹에 추가하지 않고 `sudo docker`를 사용합니다.

## 3. 코드와 운영 비밀값 설정

GitHub 저장소를 서버에 clone합니다. 아래 명령의 `<repo-url>`은 실제 저장소 주소로 바꿉니다.

```bash
git clone <repo-url> PetMap
cd PetMap
cp .env.example .env
chmod 600 .env
```

`.env`를 편집해 실제 도메인, 수신 가능한 SMTP 계정, 발신 이메일을 입력합니다. `SMTP_PORT=587`은 STARTTLS SMTP 예시입니다. 메일 제공자가 준 SMTP 서버/포트/앱 비밀번호를 넣으세요.

서버에서 `openssl rand -hex 32`를 두 번 실행해 서로 다른 값을 만들고, 결과를 각각 `MSSQL_SA_PASSWORD`와 `DB_APP_PASSWORD`의 `Aa1!` 뒤에 붙입니다. 예를 들어 `Aa1!` + 64자리 hex 문자열 형태입니다. SQL Server `sa` 비밀번호와 앱 DB 계정 비밀번호를 재사용하지 마세요. `.env`는 Git에 추가하지 않습니다.

## 4. 실행과 첫 관리자 계정

저장소 루트에서 실행합니다.

```bash
sudo docker compose up -d --build
sudo docker compose ps
sudo docker compose logs -f app
```

앱이 시작할 때 EF Core migration을 적용하고, 새 DB를 초기화합니다. SQL Server는 `petmap_app` 계정을 만들며 앱은 `sa` 계정을 사용하지 않습니다. Caddy가 HTTPS 인증서를 발급하면 `https://<DOMAIN>`으로 접속할 수 있습니다.

먼저 사이트에서 회원가입하고 SMTP 확인 메일의 링크를 열어 계정을 확인합니다. 그 뒤 `.env`에 `ADMIN_EMAIL`을 해당 계정 이메일로 설정하고 다음 명령을 실행하면 시작 시 Admin 역할을 부여합니다.

```bash
sudo docker compose up -d
```

## 5. 업데이트, 로그, 재시작

코드를 업데이트한 뒤 앱을 다시 빌드합니다. DB, 업로드 이미지, 인증 키, Caddy 인증서는 named volume에 보존됩니다.

```bash
git pull
sudo docker compose up -d --build
sudo docker compose logs --tail=100 app
```

Docker는 부팅 시 자동 시작하고 각 컨테이너는 `restart: unless-stopped`로 실행됩니다. `docker compose down -v`는 모든 DB/업로드/인증서 볼륨을 삭제하므로 운영 서버에서 실행하지 마세요.

## 데이터 및 복구 주의사항

- 처음 시작하면 새 SQL Server DB가 만들어집니다. 기존 개발 PC의 LocalDB 데이터는 자동으로 복사되지 않습니다. 기존 장소·회원 데이터를 보존해야 하면 배포 전에 별도의 백업/복원 절차가 필요합니다.
- Lightsail 자동 스냅샷은 서버 복구용입니다. 별도 SQL 백업도 준비하고 복구 연습을 하세요. 스냅샷은 현재 안내 기준 저장 용량에 따라 추가 과금됩니다.
- Docker 볼륨은 인스턴스 디스크에 있습니다. 인스턴스 삭제 전에 스냅샷과 별도 백업을 확인하세요.
- 4GB 인스턴스에서 메모리 부족 또는 SQL 응답 지연이 생기면 먼저 Lightsail 8GB 플랜으로 올립니다. Express의 10GB DB 제한에 접근하면 DB 제품/구조를 다시 결정해야 합니다.
