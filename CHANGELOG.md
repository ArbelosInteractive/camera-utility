# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Released]

## [1.0.0] - 2024-07-21
### Added
- Initial package setup.

## [1.0.1] - 2024-09-06
### Fixed
- Fixed an issue where if the original virtual camera was set through inspector, it would try to initialize it before unity camera's active virtual camera is setup resulting in a null ref.
- Now it waits until a unity camera has been initialized with cinemachine brain.

## [1.0.2] - 2024-09-06
### Updated
- Updated DollyPath State such that if a focusObject is assigned, the camera will look at it the whole travel path, otherwise camera looks at the path direction.

## [1.0.3] - 2024-09-20
### Updated
- Updated the cinemachine behavior camera setup to be called on awake.