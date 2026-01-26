# Tekoälyn tuottaman koodin arviointi ja parannukset

## 1. Mitä tekoäly teki hyvin?

Tekoäly onnistui rakentamaan toimivan perus-API:n kokonaisuuden.

* Tarvittavat osat, kuten `Program.cs`, endpointit ja palvelut, tulivat heti käyttökelpoiseen muotoon.
* Domain-malli oli selkeä, ja `Room`- ja `Reservation`-entiteetit oli määritelty johdonmukaisesti.
* Keskeiset liiketoimintasäännöt oli toteutettu:

  * päällekäisyyksien estäminen
  * aikarajat
  * varauksen luonti
  * perusvalidoinnit
* In-memory-varastot olivat valmiina käyttöön.
* Kerrosjako oli luettavaa ja helposti laajennettavaa.

**Yhteenveto:**
Tekoäly tuotti toimivan, testattavan ja laajennettavan perusratkaisun, johon oli helppo jatkaa refaktorointeja ja tarkentaa validointeja.

---

## 2. Mitä tekoäly teki huonosti?

* API-validoinnit olivat puutteellisia:

  * `fromUtc` / `toUtc` -järjestystä ei validoitu listauksessa
  * ei-UTC-aikaleimat sallittiin
  * `Location`-headerista puuttui uuden resurssin id
* `Result`-tyyppi ei ollut null-turvallinen, mikä voi johtaa NRE-virheisiin.
* Async-sopimus oli turha (`Task.FromResult`-polut), mikä lisäsi kompleksisuutta ilman todellista hyötyä.

---

## 3. Mitkä olivat tärkeimmät parannukset, jotka teit tekoälyn tuottamaan koodiin ja miksi?

### 3.1 Location-headerin korjaus (201 Created)

* Lisäsin resurssin id:n `Location`-headeriin REST-käytännön mukaisesti.
* Tämä mahdollistaa asiakkaalle juuri luodun varauksen hakemisen.

---

### 3.2 Validointien laajennus (fromUtc / toUtc + UTC-offset)

* Estää epäloogiset pyynnöt, kuten:

  * `fromUtc > toUtc`
  * ei-UTC-muotoiset aikaleimat
* Parantaa virheiden selkeyttä palauttamalla virheellisistä pyynnöistä `400 Bad Request`.

---

### 3.3 Result-tyypin null-turvallisuus

* Poistin `Value!`-pakotukset.
* Lisäsin geneerisen rajoitteen `where T : notnull`.
* Lisäsin ajonaikaisen tarkistuksen.

> **Tavoite:** onnistunut tulos ei voi koskaan sisältää null-arvoa.

---

### 3.4 Synkroninen Create-palvelu

* Poistin turhan `Task.FromResult`-kutsun.
* Tein metodista aidosti synkronisen.

> Tämä selkeyttää koodia ja poistaa tarpeettoman async-overheadin.

---

### 3.5 Huonelistasta immuuttinen ja säikeiturvallinen

* Muutin `_rooms`-kokoelman tyypiksi `IReadOnlyList`.
* Palautan listan `AsReadOnly()`-kääreen kautta.

> Tämä estää ulkopuoliset muutokset ja parantaa säikeiturvallisuutta.

---

### 3.6 Kerrosarkkitehtuuri ja vastuunjako

* Erotin seuraavat kerrokset toisistaan:

  * API
  * Application
  * Domain
  * Infrastructure

* Pidin domain-kerroksen puhtaana:

  * ei DTO-riippuvuuksia
  * ei tallennuslogiikkaa

* Siirsin endpoint-määrittelyt pois `Program.cs`:stä.

* Abstrahoin tallennuksen rajapinnan taakse.

> Tämä mahdollistaa InMemory-toteutuksen vaihtamisen EF Coreen ilman, että liiketoimintasääntöjä tai sovelluslogiikkaa tarvitsee muuttaa.

---

## Yhteenveto

Nämä parannukset tekivät koodista:

* REST-yhteensopivamman
* turvallisemman
* helpommin testattavan
* laajennettavamman
* arkkitehtuurisesti selkeämmän

ja valmistivat projektin mahdollisiin muutoksiin, kuten tallennusratkaisun vaihtamiseen (InMemory → EF Core) ilman business-logiikan muuttamista.
