# Медицинская информационная система

#### REST API на Asp.Net Core 8 для медицинской информационной системы, предназначенной для врачей абстрактного медицинского учреждения. Включает в себя следующие аспекты:
- регистрация новых врачей;
- фиксация пациентов и их осмотров;
- фиксация консультаций для пациентов;
- работа с медицинским справочником МКБ-10.

#### Приложение является монолитным. Используются следующие технологии и подходы:
- **EntityFramework Core + PostgreSQL** для персистентного хранения данных;
- **JWT Tokens** для авторизации в системе;
- **Quartz** для фоновых задач;
- **MailKit** для отправки email уведомлений врачам о пациентах, пропустивших запланированное посещение.

## Перед запуском убедитесь, что у вас установлен PostgreSQL на вашей машине

#### Для успешного запуска приложения необходимо в директории WebApi создать файл конфигурации appsettings.json и поместить туда следующие настройки:
```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },

  "Smtp": {
    "Email": "<ваша почта>",
    "Password": "<ваш пароль для SMTP>"
  },

  "ConnectionStrings": {
    "AppDbContext": "Host=localhost;Database=MISdb;Username=postgres;Password=123"
  },

  "Jwt": {
    "Issuer": "MIS.back",
    "Audience": "MIS.client",
    "Secret": "Very secret-secret secret secret secret key",
    "ExpireInMinutes": 60
  },
  "Scheduler": {
    "EmailSenderIntervalInMinutes": 1,
    "JwtRemoverIntervalInMinutes": 60
  }
}
```
#### Также необходимо сделать миграцию, заполнить БД специальностями для врачей (эндпоинт POST /api/dictionary/speciality) и импортировать из BusinessLogic/ImportData/ICD10.json справочник в БД (эндпоинт POST /api/dictionary/icd10/import)
