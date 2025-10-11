-- Test MySQL Connection
-- Run this in MySQL Workbench to test connection

-- Show current user
SELECT USER() as CurrentUser;

-- Show current database
SELECT DATABASE() as CurrentDatabase;

-- Show all databases you have access to
SHOW DATABASES;

-- Show server version
SELECT VERSION() as ServerVersion;

-- Test a simple query
SELECT 'Connection successful!' as Status, NOW() as CurrentTime;

