﻿select id, content from test_doc where changed is null or changed > @seed limit @offset, @limit