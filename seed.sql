INSERT INTO "Users" (
  "Id",
  "Name",
  "Email",
  "PasswordHash",
  "Role",
  "SchoolId"
)
VALUES (
  gen_random_uuid(),
  'TI.APSe',
  'ti.apse@adventistas.org',
  'T1.4ps3#Secure',
  0,
  NULL
);
