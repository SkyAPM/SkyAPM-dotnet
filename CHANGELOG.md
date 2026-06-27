# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.3.0]

### Added

- .NET 10 support (`net10.0` target framework).

### Changed

- Drop the end-of-life target frameworks `netcoreapp3.1`, `net5.0` and `net6.0`. The agent now targets `net8.0` and `net10.0` (the active LTS releases); the foundational libraries (`SkyApm.Core`, `SkyApm.Abstractions`, `SkyApm.Transport.Grpc`, `SkyApm.Transport.Kafka`) keep a `netstandard2.0` compatibility floor.
- CI now builds and tests on both .NET 8 and .NET 10.

## [1.1.0]

### Added

- Support ignore path feature.
- Support header propagation path configuration for `HttpClient` diagnostic component.

### Changed
- Improve performance of TracingDiagnosticObserver.
- Remove unnecessary span log in `AspNetCore` diagnostic component.
- Append ip to the instancename to make it more meaningful.
- Remove query party from the span operation name for `HttpClient` diagnostic component. 

### Fixed
- Grpc throws exception when null string in span tag or log.

