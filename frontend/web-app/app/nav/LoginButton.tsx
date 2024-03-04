'use client'

import { Button } from 'flowbite-react'
import React from 'react'
import {signIn} from 'next-auth/react'


export default function LoginButton() {
  return (
    <Button outline onClick={async () => await signIn('id-server', {callbackUrl: `http://localhost:3000`})}>
        Login
    </Button>
  )
}
