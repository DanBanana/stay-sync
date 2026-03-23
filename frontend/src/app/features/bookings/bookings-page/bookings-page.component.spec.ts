import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideMockStore, MockStore } from '@ngrx/store/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ActivatedRoute } from '@angular/router';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { BookingsPageComponent } from './bookings-page.component';
import { BookingsActions } from '../../../store/bookings/bookings.actions';

describe('BookingsPageComponent', () => {
  let component: BookingsPageComponent;
  let fixture: ComponentFixture<BookingsPageComponent>;
  let store: MockStore;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [BookingsPageComponent],
      imports: [RouterTestingModule],
      providers: [
        provideMockStore({ initialState: { bookings: { bookings: [], loading: false, error: null } } }),
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => 'room-1' } } } },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    store = TestBed.inject(MockStore);
    fixture = TestBed.createComponent(BookingsPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should dispatch loadBookingsByRoom on init', () => {
    const dispatchSpy = spyOn(store, 'dispatch');
    component.ngOnInit();
    expect(dispatchSpy).toHaveBeenCalledWith(BookingsActions.loadBookingsByRoom({ roomId: 'room-1' }));
  });
});
