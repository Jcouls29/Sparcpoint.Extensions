# Changelog

## [1.0.3] - 2024-03-08

- Warn: Added check for null `implementationType`

## [1.0.2] - 2024-03-08

- Fix: Bug with registered open-generics not properly registering in the child service and throwing on build.

## [1.0.1] - 2024-02-13

Initial build of Sparcpoint.Extensions that includes the following:

- Feature: `WithChildServices`: Allows for sub services to be defined per parent service
- Feature; `Decorate`: Allows for decorating services (including within child service definitions)