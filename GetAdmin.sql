docker exec -it bulkload-postgres psql -U admin -d BulkLoadDB -c "SELECT current_user, current_database();"
