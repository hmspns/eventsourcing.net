# Postgres based events source for eventsourcing.net library.

## Notes

1. If you use non base type (such as int, long, Guid) for the type of aggregate id, you should create TypeConverter for it, to provide conversion from string to the type.