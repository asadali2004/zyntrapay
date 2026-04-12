# Frontend Testing Checklist

## 🧪 Manual Testing Guide

### ✅ Navigation Testing

#### Header Navigation
- [ ] Logo redirects to home page
- [ ] "Home" link works and shows active state
- [ ] "About Us" link works and shows active state
- [ ] "Contact" link works and shows active state
- [ ] "Login" button redirects to login page
- [ ] "Sign Up" button redirects to register page
- [ ] Active route is highlighted correctly

#### Footer Navigation
- [ ] All footer links are clickable
- [ ] Social media icons have hover effects
- [ ] Footer displays correctly on all pages

### ✅ Page Testing

#### Home Page (`/`)
- [ ] Hero section displays correctly
- [ ] Hero buttons are clickable
- [ ] All 6 feature cards display
- [ ] Feature cards have hover effects
- [ ] Statistics section shows 4 stats
- [ ] CTA section displays
- [ ] Page is responsive on mobile

#### About Page (`/about`)
- [ ] Hero section displays
- [ ] Story section with image placeholder
- [ ] 3 mission cards display
- [ ] 4 team member profiles show
- [ ] 4 company values display
- [ ] All sections are responsive

#### Contact Page (`/contact`)
- [ ] Contact form displays all fields
- [ ] Form validation works (required fields)
- [ ] Email validation works
- [ ] 4 contact info cards display
- [ ] Social media icons work
- [ ] FAQ section shows 4 questions
- [ ] Form is responsive

### ✅ Authentication Testing

#### Login Page (`/login`)
- [ ] Form displays correctly
- [ ] Email validation works
- [ ] Password validation works
- [ ] "Remember me" checkbox works
- [ ] "Forgot password" link is visible
- [ ] Error messages display for invalid input
- [ ] Loading state shows during submission
- [ ] Successful login redirects to dashboard
- [ ] Failed login shows error message
- [ ] "Sign up" link redirects to register
- [ ] Google login button is visible

#### Register Page (`/register`)
- [ ] All form fields display
- [ ] Full name validation works
- [ ] Email validation works
- [ ] Phone number field is optional
- [ ] Password validation (min 6 chars)
- [ ] Confirm password validation
- [ ] Password mismatch error shows
- [ ] Terms checkbox validation
- [ ] Success message displays
- [ ] Error messages display
- [ ] Redirects to login after success
- [ ] "Sign in" link works
- [ ] Google signup button visible

### ✅ Dashboard Testing

#### User Dashboard (`/dashboard`)
- [ ] Requires authentication (redirects if not logged in)
- [ ] User name displays in greeting
- [ ] 4 stat cards display
- [ ] Transaction list shows 4 items
- [ ] Transaction icons display
- [ ] Positive/negative amounts colored correctly
- [ ] 4 quick action cards display
- [ ] Action cards have hover effects
- [ ] Logout button works
- [ ] Dashboard is responsive

#### Admin Dashboard (`/admin/dashboard`)
- [ ] Requires admin role
- [ ] System stats display (4 cards)
- [ ] Pending KYC section shows
- [ ] KYC cards display correctly
- [ ] "Approve" button works
- [ ] "Reject" button works
- [ ] "View Document" link works
- [ ] User table displays
- [ ] User status badges show correctly
- [ ] Toggle user status works
- [ ] Logout button works
- [ ] Loading state displays
- [ ] Empty state shows when no KYCs

### ✅ API Integration Testing

#### Authentication APIs
- [ ] Register API call works
- [ ] Login API call works
- [ ] Token is stored in localStorage
- [ ] Token is sent in API requests
- [ ] Refresh token works on 401
- [ ] Logout clears tokens
- [ ] Protected routes check authentication

#### Admin APIs
- [ ] Get pending KYCs works
- [ ] Review KYC works
- [ ] Get all users works
- [ ] Toggle user status works
- [ ] Get dashboard stats works
- [ ] Error handling works for failed requests

### ✅ Responsive Design Testing

#### Mobile (< 640px)
- [ ] Header navigation adapts
- [ ] Hero section stacks vertically
- [ ] Feature grid becomes single column
- [ ] Stats grid becomes single column
- [ ] Forms are full width
- [ ] Buttons are full width
- [ ] Footer stacks vertically
- [ ] Tables scroll horizontally

#### Tablet (640px - 968px)
- [ ] Navigation shows properly
- [ ] Grids become 2 columns
- [ ] Images scale correctly
- [ ] Forms maintain good width
- [ ] Cards stack appropriately

#### Desktop (> 968px)
- [ ] Full layout displays
- [ ] Multi-column grids work
- [ ] Hover effects work
- [ ] Spacing is appropriate

### ✅ Form Validation Testing

#### Login Form
- [ ] Empty email shows error
- [ ] Invalid email format shows error
- [ ] Empty password shows error
- [ ] Errors clear when corrected
- [ ] Submit disabled when invalid

#### Register Form
- [ ] All required fields validated
- [ ] Email format validated
- [ ] Password length validated
- [ ] Password match validated
- [ ] Terms checkbox validated
- [ ] Errors show on blur
- [ ] Submit disabled when invalid

#### Contact Form
- [ ] Name validation works
- [ ] Email validation works
- [ ] Subject validation works
- [ ] Message validation works
- [ ] Phone is optional

### ✅ UI/UX Testing

#### Colors
- [ ] No blue colors used
- [ ] No gradients used
- [ ] Primary color (terracotta) used correctly
- [ ] Secondary color (sage green) used correctly
- [ ] Accent color (warm sand) used correctly
- [ ] Text colors have good contrast

#### Interactions
- [ ] Buttons have hover effects
- [ ] Cards have hover effects
- [ ] Links change color on hover
- [ ] Focus states are visible
- [ ] Transitions are smooth (0.3s)
- [ ] No jarring animations

#### Typography
- [ ] Font sizes are readable
- [ ] Headings have proper hierarchy
- [ ] Line height is comfortable
- [ ] Text colors are accessible

#### Spacing
- [ ] Consistent padding/margins
- [ ] White space is generous
- [ ] Sections are well separated
- [ ] Content is not cramped

### ✅ Browser Testing

#### Chrome
- [ ] All features work
- [ ] Styles render correctly
- [ ] No console errors

#### Firefox
- [ ] All features work
- [ ] Styles render correctly
- [ ] No console errors

#### Safari
- [ ] All features work
- [ ] Styles render correctly
- [ ] No console errors

#### Edge
- [ ] All features work
- [ ] Styles render correctly
- [ ] No console errors

### ✅ Performance Testing

- [ ] Page loads quickly
- [ ] Images load properly
- [ ] No layout shifts
- [ ] Smooth scrolling
- [ ] Fast navigation between pages
- [ ] API calls complete quickly

### ✅ Error Handling

- [ ] Network errors show messages
- [ ] API errors display properly
- [ ] 404 redirects to home
- [ ] Invalid routes redirect
- [ ] Form errors are clear
- [ ] Loading states prevent double-submit

### ✅ Accessibility Testing

- [ ] All images have alt text (or use emojis)
- [ ] Forms have proper labels
- [ ] Buttons have descriptive text
- [ ] Color contrast is sufficient
- [ ] Keyboard navigation works
- [ ] Focus indicators are visible
- [ ] Error messages are descriptive

## 🐛 Common Issues to Check

### Authentication Issues
- [ ] Token not being sent with requests
- [ ] Token not being stored
- [ ] Refresh token not working
- [ ] Logout not clearing tokens
- [ ] Protected routes accessible without auth

### API Issues
- [ ] CORS errors
- [ ] Wrong API URL
- [ ] Missing environment configuration
- [ ] Request/response format mismatch
- [ ] Error responses not handled

### UI Issues
- [ ] Styles not loading
- [ ] Components not rendering
- [ ] Routes not working
- [ ] Forms not submitting
- [ ] Validation not working

### Responsive Issues
- [ ] Layout breaks on mobile
- [ ] Text too small
- [ ] Buttons too small
- [ ] Horizontal scroll appears
- [ ] Images overflow

## 📝 Testing Notes

### Test Data
```
Regular User:
Email: user@test.com
Password: test123

Admin User:
Email: admin@test.com
Password: admin123
```

### API Endpoints to Test
```
POST /gateway/auth/register
POST /gateway/auth/login
GET  /gateway/admin/dashboard
GET  /gateway/admin/kyc/pending
GET  /gateway/admin/users
```

### Browser DevTools
- Check Console for errors
- Check Network tab for API calls
- Check Application tab for localStorage
- Use Responsive Design Mode

## ✅ Final Checklist

- [ ] All pages load without errors
- [ ] All navigation works
- [ ] All forms validate correctly
- [ ] All API calls work
- [ ] All styles render correctly
- [ ] Responsive design works
- [ ] No console errors
- [ ] No broken links
- [ ] Authentication flow works
- [ ] Admin features work
- [ ] Design matches requirements (no blue, no gradients)
- [ ] UI looks human-designed

## 🎉 Ready for Production

Once all items are checked, the frontend is ready for production deployment!
