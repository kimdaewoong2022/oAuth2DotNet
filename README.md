# oAuth2DotNet

# 11. 소스 컨트롤에 프로젝트 추가

#### **2-11. 소스 컨트롤에 프로젝트 추가**

프로젝트 작업을 시작하기 전에 소스 컨트롤 시스템에 프로젝트를 추가하는 것은 매우 중요합니다. 이를 통해 코드가 손상되거나 이전 상태로 되돌아가야 할 경우를 대비할 수 있습니다.

1. **소스 컨트롤 사용의 중요성**: 소스 컨트롤을 사용하면 코드 변경 사항을 추적하고, 필요한 경우 이전 버전으로 쉽게 되돌릴 수 있습니다.

2. **Git을 사용한 소스 컨트롤 추가**:
   - Visual Studio의 하단에서 '소스 컨트롤에 추가(Add to Source Control)' 옵션을 선택합니다.
   - Git 옵션을 선택하면 관련 대화 상자가 열립니다.
   - GitHub 계정에 로그인한 상태에서 리포지토리 이름을 `MagicVilla_API`로 설정합니다.
   - 처음에는 비공개 리포지토리로 설정하되, 코드가 출시 준비가 되면 공개 리포지토리로 전환할 계획입니다. 이를 통해 변경 사항을 비디오별로 검토할 수 있습니다.
   - 리포지토리 생성 및 푸시를 완료한 후 GitHub 계정에서 새로운 리포지토리 `MagicVilla_API`를 확인하여 동기화 상태를 검증합니다.

이 과정을 통해 개발자는 프로젝트의 버전 관리를 시작할 수 있으며, 코드의 안정성과 관리 효율성을 높일 수 있습니다. 소스 컨트롤에 프로젝트를 추가하는 것은 협업과 프로젝트 관리에 있어 필수적인 단계입니다.


---
# 12. 코드 정리

#### **12. 코드 정리**

이 섹션에서는 API 프로젝트를 위해 필요한 기본 파일 설정과 불필요한 파일 제거 과정을 다룹니다.

1. **불필요한 파일 제거**: 프로젝트를 새로 시작하기 위해, 기본으로 제공된 `WeatherForecast` 클래스 파일과 관련 컨트롤러를 삭제합니다. 이를 통해 프로젝트를 깔끔하게 정리하고 필수 파일만 남깁니다.

2. **필수 파일 유지**:
   - **Controllers 폴더**: API 엔드포인트를 처리할 컨트롤러를 포함합니다.
   - **appsettings.json**: 애플리케이션 설정을 저장합니다.
   - **Program.cs**: 애플리케이션의 진입점을 정의하고, 애플리케이션을 구성합니다.

이 과정을 통해 프로젝트 내에 필수적인 파일만 남기고, 불필요한 파일을 제거하여 코드의 가독성과 관리 효율성을 높였습니다. 프로젝트의 초기 설정 단계에서 불필요한 파일을 정리하는 것은 프로젝트 구조를 명확하게 하고, 향후 유지보수를 용이하게 하는 데 도움이 됩니다.



---
# 13. VillaAPIController 클래스 생성


#### **13. VillaAPIController 클래스 생성**

이 섹션에서는 첫 번째 API 컨트롤러인 `VillaAPIController` 클래스를 생성하는 과정을 다룹니다.

1. **컨트롤러 폴더 확인**: 프로젝트 내에 컨트롤러 폴더가 보이지 않을 경우, '모든 파일 표시' 옵션을 사용하여 폴더를 확인할 수 있습니다. 폴더가 비어 있기 때문에 기본적으로는 보이지 않습니다.

2. **API 컨트롤러 추가**:
   - 컨트롤러 폴더에 우클릭하여 '클래스 추가'를 선택합니다. MVC 컨트롤러가 아닌, 기본 API 컨트롤러를 원합니다.
   - 클래스 파일을 추가하고, 이를 API 컨트롤러로 만들기 위해 클래스 이름에 'Controller'를 포함시킵니다. 예: `VillaAPIController`.

3. **컨트롤러 클래스 설정**:
   - 생성된 `VillaAPIController` 클래스는 `ControllerBase` 클래스를 상속받아야 합니다. 이를 통해 .NET 애플리케이션에서 컨트롤러와 관련된 일반적인 메서드를 사용할 수 있습니다.
   - `APIController` 어트리뷰트를 클래스에 추가하여 이 클래스가 API 컨트롤러임을 명시합니다.

4. **주요 포인트**:
   - `ControllerBase` 상속: API 컨트롤러의 기능을 활성화합니다.
   - `APIController` 어트리뷰트: 클래스가 API 컨트롤러로 동작하도록 설정합니다.
   - MVC 뷰 지원은 포함하지 않음: 현재 API 애플리케이션에서는 MVC 뷰를 사용하지 않으므로, 관련 지원을 추가하지 않습니다.

이 과정을 통해 기본적인 API 컨트롤러를 성공적으로 생성했습니다. 현재 컨트롤러는 아직 엔드포인트를 포함하고 있지 않지만, API 컨트롤러로서의 기본 구조를 갖추고 있습니다. 이후 비디오에서는 이 컨트롤러에 엔드포인트를 추가하는 방법을 다룰 예정입니다.

```csharp
using Microsoft.AspNetCore.Mvc;

[ApiController]
public class VillaAPIController : ControllerBase
{
    // 여기에 엔드포인트 메서드를 추가할 수 있습니다.
}
```

위 코드는 `VillaAPIController` 클래스의 기본 구조를 보여줍니다. `ControllerBase`를 상속받고, `APIController` 어트리뷰트를 사용하여 API 컨트롤러로 설정했습니다.


---
# 14. 첫 번째 API 엔드포인트 - HTTPGET



#### **14. 첫 번째 API 엔드포인트 - HTTPGET**

이 섹션에서는 첫 번째 API 엔드포인트를 생성하는 과정을 다룹니다.

1. **모델 생성**:
   - `Models` 폴더를 생성하고, 이 폴더 내에 `Villa` 모델을 추가합니다.
   - `Villa` 모델에는 두 개의 속성이 포함됩니다: `ID`(빌라의 ID)와 `Name`(빌라의 이름).

2. **API 컨트롤러 업데이트**:
   - `VillaAPIController` 내에 `GetVillas` 메서드를 생성하여 `Villa` 객체의 리스트를 반환합니다.
   - 반환 타입은 `IEnumerable<Villa>`이며, 두 개의 빌라 객체(`Pool Villa`와 `Beach Villa`)를 포함하는 새 리스트를 반환합니다.

3. **라우팅 설정**:
   - API 컨트롤러에 `Route` 어트리뷰트를 추가하여 엔드포인트의 라우트를 정의합니다. 일반적으로 `api/[컨트롤러명]` 형식을 사용합니다.

4. **HTTP GET 어트리뷰트 추가**:
   - 엔드포인트가 HTTP GET 요청을 처리하도록 `HttpGet` 어트리뷰트를 `GetVillas` 메서드에 추가합니다.

5. **Swagger를 통한 테스트**:
   - Swagger UI를 사용하여 생성된 GET 엔드포인트를 테스트합니다. 'Try it out' 버튼을 클릭하고 'Execute'를 누르면, 정의된 두 개의 빌라 객체가 반환됩니다.

6. **결과 확인**:
   - 엔드포인트는 예상대로 작동하며, Swagger UI를 통해 API를 쉽게 테스트하고 문서화할 수 있습니다.

```csharp
// Villa 모델
public class Villa
{
    public int ID { get; set; }
    public string Name { get; set; }
}

// VillaAPIController 내 GetVillas 메서드
[HttpGet]
public IEnumerable<Villa> GetVillas()
{
    return new List<Villa>
    {
        new Villa { ID = 1, Name = "Pool Villa" },
        new Villa { ID = 2, Name = "Beach Villa" }
    };
}
```

이 코드는 `Villa` 모델과 `VillaAPIController` 내 `GetVillas` 메서드의 기본 구조를 보여줍니다. `HttpGet` 어트리뷰트를 사용하여 HTTP GET 요청을 처리하고, Swagger를 통해 API 엔드포인트를 쉽게 테스트할 수 있습니다.

---
# 15. 라우트에서 컨트롤러 이름 사용



#### **15. 라우트에서 컨트롤러 이름 사용하기**

이 섹션에서는 API 엔드포인트의 라우트에 컨트롤러 이름을 자동으로 포함시키는 방법을 설명합니다.

1. **라우트 설정 개선**:
   - `VillaAPIController`의 라우트 설정에 `[controller]` 플레이스홀더를 사용하여 컨트롤러 이름(`VillaAPI`)을 자동으로 라우트 경로에 포함시킬 수 있습니다. 예를 들어, `[Route("api/[controller]")]`와 같이 사용됩니다.

2. **자동 라우트 이름 적용**:
   - `[controller]` 플레이스홀더를 사용하면, 컨트롤러의 이름이 변경되더라도 라우트 경로가 자동으로 업데이트됩니다. 이는 유지보수 시 편리할 수 있으나, 외부에서 API를 사용하는 클라이언트에게 영향을 줄 수 있습니다.

3. **하드코딩 라우트의 중요성**:
   - 컨트롤러 이름이 변경되어도 라우트 경로가 변경되지 않도록 하드코딩하는 방식을 선호합니다. 이렇게 하면 API의 엔드포인트를 사용하는 클라이언트에게 라우트 변경 사항을 통보할 필요가 없어집니다.

4. **실습 예시**:
   - 라우트를 하드코딩하는 것과 `[controller]` 플레이스홀더를 사용하는 방법을 모두 보여주며, 각 방법의 장단점을 설명합니다.

```csharp
[Route("api/[controller]")]
public class VillaAPIController : ControllerBase
{
    // 엔드포인트 메서드들...
}
```

위 코드 예시에서 `[controller]` 플레이스홀더는 `VillaAPIController`의 `VillaAPI` 부분을 자동으로 라우트 경로에 적용합니다. 하지만, 라우트 경로를 명시적으로 지정하는 것이 더 안정적인 API 버전 관리를 가능하게 합니다.

이 섹션은 API 개발 시 라우트 설정에 대한 중요한 고려 사항을 제공하며, 향후 API의 유지보수와 클라이언트와의 호환성을 고려할 때 유용한 지침을 제시합니다.

---
# 16. VillaDTO 추가



#### **16. VillaDTO 추가**

이 섹션에서는 실제 프로덕션 환경에서 API를 구현할 때 모델 대신 DTO(Data Transfer Object)를 사용하는 방법을 다룹니다.

1. **DTO의 역할**:
   - DTO는 데이터베이스 모델(엔티티)과 API를 통해 외부에 노출되는 데이터 사이의 중간자 역할을 합니다. 이를 통해 클라이언트에게 필요한 데이터만 선택적으로 전달할 수 있으며, 데이터베이스 구조의 변경이 API 사용자에게 직접적인 영향을 미치지 않도록 합니다.

2. **VillaDTO 구현**:
   - `Models` 폴더 내에 `DTO` 폴더를 추가하고, 이 폴더 내에 `VillaDTO` 클래스를 생성합니다.
   - `VillaDTO` 클래스에는 `Villa` 모델과 동일한 `ID`와 `Name` 속성을 추가합니다. 추가적으로, 데이터베이스에는 존재하지만 DTO를 통해 클라이언트에게 노출하지 않을 `CreatedDate` 속성을 예로 들 수 있습니다.

3. **API 컨트롤러에서 DTO 사용**:
   - API 컨트롤러에서는 이제 `Villa` 모델 대신 `VillaDTO`를 반환하도록 수정합니다. 이를 통해 클라이언트에게 `ID`와 `Name` 정보만 제공하고, `CreatedDate`와 같은 내부 정보는 숨깁니다.

4. **실습 예시**:
   ```csharp
   public class VillaDTO
   {
       public int ID { get; set; }
       public string Name { get; set; }
   }

   [HttpGet]
   public IEnumerable<VillaDTO> GetVillas()
   {
       return new List<VillaDTO>
       {
           new VillaDTO { ID = 1, Name = "Pool Villa" },
           new VillaDTO { ID = 2, Name = "Beach Villa" }
       };
   }
   ```
   - 위 코드는 `VillaDTO`를 사용하여 클라이언트에게 빌라의 `ID`와 `Name`만 노출하는 방법을 보여줍니다.

5. **결론**:
   - DTO를 사용함으로써 API의 유연성을 높이고, 클라이언트에게 노출되는 데이터를 세밀하게 제어할 수 있습니다. 이는 애플리케이션의 보안과 유지보수성을 향상시키는 중요한 방법입니다.

이 섹션은 API 개발에서 DTO의 중요성과 구현 방법을 설명하여, 개발자가 데이터 노출을 효과적으로 관리할 수 있도록 돕습니다.

---
# 17. Villa 데이터 스토어



#### **17. Villa 데이터 저장소**

이 섹션에서는 API 내에서 CRUD(생성, 읽기, 업데이트, 삭제) 작업을 수행하기 위한 데이터 저장소를 구현하는 방법을 다룹니다.

1. **데이터 저장소의 필요성**:
   - 실제 애플리케이션에서는 데이터베이스를 사용하여 데이터를 저장합니다. 하지만, 본 강의에서는 데이터베이스 설정을 단순화하기 위해 임시 데이터 저장소를 사용합니다.

2. **VillaStore 클래스 구현**:
   - `Data` 폴더를 생성하고, 이 폴더 내에 `VillaStore`라는 정적 클래스를 추가합니다.
   - `VillaStore` 클래스 내에 `VillaDTO` 객체의 리스트를 정적 멤버로 선언하여, 애플리케이션 전반에서 사용할 수 있는 공통 데이터 저장소를 제공합니다.

3. **VillaStore 사용 예시**:
   ```csharp
   public static class VillaStore
   {
       public static List<VillaDTO> VillaList = new List<VillaDTO>
       {
           new VillaDTO { ID = 1, Name = "Pool Villa" },
           new VillaDTO { ID = 2, Name = "Beach Villa" }
       };
   }
   ```
   - 위 코드는 `VillaDTO` 객체의 리스트를 초기화하고, 두 개의 빌라 정보를 저장합니다.

4. **API 컨트롤러에서 VillaStore 사용**:
   - `VillaAPIController`에서 `VillaStore`의 `VillaList`를 사용하여 클라이언트에게 빌라 정보를 반환합니다.
   - 이를 통해 데이터베이스 대신 임시 데이터 저장소를 사용하여 데이터를 관리할 수 있습니다.

5. **테스트 및 검증**:
   - 애플리케이션을 실행하고 Swagger UI를 통해 `GetVillas` 엔드포인트를 테스트합니다. `VillaStore`에 저장된 빌라 정보가 성공적으로 반환되는지 확인합니다.

이 방법은 개발 초기 단계에서 데이터베이스 구성 없이 데이터 관리를 단순화하고, API 개발 및 테스트를 용이하게 합니다. 추후 데이터베이스를 도입하면, `VillaStore`를 실제 데이터베이스 연동 로직으로 대체할 수 있습니다.

```csharp
// VillaAPIController 내에서 VillaStore 사용
[HttpGet]
public IEnumerable<VillaDTO> GetVillas()
{
    return VillaStore.VillaList;
}
```

위 코드는 `VillaAPIController`에서 `VillaStore`의 `VillaList`를 반환하는 방법을 보여줍니다. 이를 통해 클라이언트는 저장된 빌라 정보에 접근할 수 있습니다.

---
# 18. 개별 Villa 가져오기



#### **18. 개별 Villa 조회**

이 섹션에서는 ID를 기반으로 단일 Villa 정보를 조회하는 API 엔드포인트를 구현하는 방법을 다룹니다.

1. **개별 Villa 조회 엔드포인트 추가**:
   - 기존의 모든 Villa를 반환하는 `GetVillas` 메서드와 별개로, 특정 ID를 가진 Villa만을 반환하는 `GetVilla` 메서드를 추가합니다.
   - `GetVilla` 메서드는 매개변수로 ID를 받아, 해당 ID와 일치하는 Villa를 반환합니다. 이때 LINQ의 `FirstOrDefault` 메서드를 사용하여 조건에 맞는 첫 번째 Villa를 찾거나, 없으면 null을 반환합니다.

2. **라우트 설정**:
   - 두 개의 GET 메서드(`GetVillas`와 `GetVilla`)가 있을 때, Swagger 등의 도구에서 라우트 충돌을 방지하기 위해 `GetVilla` 메서드에 `[HttpGet("{id}")]` 어트리뷰트를 추가하여 ID를 기대하는 것을 명시합니다.

3. **실습 예시**:
   ```csharp
   [HttpGet]
   public IEnumerable<VillaDTO> GetVillas()
   {
       return VillaStore.VillaList;
   }

   [HttpGet("{id}")]
   public VillaDTO GetVilla(int id)
   {
       return VillaStore.VillaList.FirstOrDefault(v => v.ID == id);
   }
   ```
   - 위 코드는 모든 Villa를 조회하는 `GetVillas` 메서드와 ID를 기반으로 특정 Villa를 조회하는 `GetVilla` 메서드를 보여줍니다. `GetVilla` 메서드는 URL에서 ID를 매개변수로 받아 해당하는 Villa를 반환합니다.

4. **테스트 및 검증**:
   - 애플리케이션을 실행하고 Swagger UI를 통해 `GetVilla` 엔드포인트를 테스트합니다. ID를 매개변수로 제공하여 특정 Villa 정보를 성공적으로 조회할 수 있습니다.

이 방법을 통해 API 사용자는 필요에 따라 전체 Villa 목록 또는 특정 Villa 정보를 조회할 수 있게 됩니다. 이는 API의 유연성을 높이고, 사용자의 다양한 요구 사항을 충족시킬 수 있는 중요한 기능입니다.

```csharp
// URL 예시: GET /api/VillaAPI/1
// 위 URL은 ID가 1인 Villa 정보를 조회합니다.
```

이 섹션은 API 개발에서 개별 리소스를 조회하는 방법과 라우트 설정의 중요성을 강조합니다.
